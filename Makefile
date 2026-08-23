COMPOSE_FILE ?= docker-compose.yml
SOLUTION := CensoDemografico.sln
FRONTEND_DIR := src/frontend/Census.WebApp/ClientApp
CONFIGURATION ?= Release

.PHONY: help env up down restart logs ps build restore test clean observability front-build urls seed-people

help: ## List available targets
	@grep -E '^[a-zA-Z0-9_-]+:.*##' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*## "}; {printf "  \033[36m%-16s\033[0m %s\n", $$1, $$2}}'

env: ## Copy .env.example to .env if missing
	@if [ ! -f .env ]; then cp .env.example .env && echo "Created .env from .env.example"; else echo ".env already exists"; fi

up: env ## Start full stack (detached, rebuild images)
	docker compose -f $(COMPOSE_FILE) up --build -d

down: ## Stop and remove containers
	docker compose -f $(COMPOSE_FILE) down

restart: down up ## Restart full stack

logs: ## Follow container logs
	docker compose -f $(COMPOSE_FILE) logs -f

ps: ## Show container status
	docker compose -f $(COMPOSE_FILE) ps

build: restore ## Build .NET solution
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

restore: ## Restore .NET packages
	dotnet restore $(SOLUTION)

test: ## Run all microservice tests
	dotnet test src/microservices/People/Census.People.Test/Census.People.Test.csproj --configuration $(CONFIGURATION)
	dotnet test src/microservices/Statistics/Census.Statistics.Test/Census.Statistics.Test.csproj --configuration $(CONFIGURATION)
	dotnet test src/microservices/FamilyTree/Census.FamilyTree.Test/Census.FamilyTree.Test.csproj --configuration $(CONFIGURATION)
	dotnet test src/microservices/Identity/Census.Identity.Test/Census.Identity.Test.csproj --configuration $(CONFIGURATION)

clean: down ## Remove containers, volumes, and .NET build artifacts
	dotnet clean $(SOLUTION) --configuration $(CONFIGURATION)

observability: env ## Start stack with observability profile (Grafana, Jaeger, Prometheus)
	docker compose -f $(COMPOSE_FILE) --profile observability up --build -d

front-build: ## Build React frontend (Vite)
	cd $(FRONTEND_DIR) && npm ci && npm run build

urls: ## Print service URLs
	@echo ""
	@echo "Application:      http://localhost:8080"
	@echo "Identity API:       http://localhost:5004"
	@echo "People API:         http://localhost:5001"
	@echo "FamilyTree API:     http://localhost:5002"
	@echo "Statistics API:     http://localhost:5003"
	@echo "Swagger (Identity): http://localhost:5004/swagger"
	@echo "Swagger (People):   http://localhost:5001/swagger"
	@echo "Admin login:        admin@censo.local / Admin@12345"
	@echo "RabbitMQ Management http://localhost:15672  (guest/guest)"
	@echo "Neo4j Browser:      http://localhost:7474  (neo4j/test)"
	@echo "Grafana:            http://localhost:3000  (admin/admin, observability profile)"
	@echo "Prometheus:         http://localhost:9090  (observability profile)"
	@echo "Jaeger:             http://localhost:16686 (observability profile)"
	@echo ""

seed-people: ## Seed ~100 test people via API (requires stack running)
	@FRONT_IP=$$(docker inspect frontend --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' 2>/dev/null); \
	if curl -sf http://localhost:8080 >/dev/null 2>&1; then \
	  node scripts/seed-people.mjs; \
	elif [ -n "$$FRONT_IP" ]; then \
	  echo "Using frontend container at http://$$FRONT_IP:8080"; \
	  CENSUS_BASE_URL=http://$$FRONT_IP:8080 node scripts/seed-people.mjs; \
	else \
	  node scripts/seed-people.mjs; \
	fi
