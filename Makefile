COMPOSE_FILE ?= docker-compose.yml
SOLUTION := CensoDemografico.sln
FRONTEND_DIR := src/frontend/Census.WebApp/ClientApp
CONFIGURATION ?= Release

.PHONY: help env up down restart logs ps build restore test test-unit test-integration lint format clean observability front-build urls seed-people

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

test: ## Run all tests
	dotnet test $(SOLUTION) --configuration $(CONFIGURATION)

test-unit: ## Run unit tests only
	dotnet test $(SOLUTION) --configuration $(CONFIGURATION) --filter "FullyQualifiedName~.Unit."

test-integration: ## Run integration tests only
	dotnet test $(SOLUTION) --configuration $(CONFIGURATION) --filter "FullyQualifiedName~.Integration."

lint: ## Verify formatting (dotnet format)
	dotnet format $(SOLUTION) --verify-no-changes

format: ## Apply dotnet format
	dotnet format $(SOLUTION)

clean: down ## Remove containers and .NET build artifacts
	dotnet clean $(SOLUTION) --configuration $(CONFIGURATION)

observability: env ## Start stack with observability profile
	OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317 \
	docker compose -f $(COMPOSE_FILE) --profile observability up --build -d

front-build: ## Build React frontend (Vite)
	cd $(FRONTEND_DIR) && npm ci && npm run build

urls: ## Print service URLs (demo credentials are DEVELOPMENT ONLY)
	@echo ""
	@echo "Application:        http://localhost:8080"
	@echo "RabbitMQ Management http://localhost:15672"
	@echo "Neo4j Browser:      http://localhost:7474"
	@echo "Grafana:            http://localhost:3000  (observability profile)"
	@echo "Prometheus:         http://localhost:9090  (observability profile)"
	@echo "Jaeger:             http://localhost:16686 (observability profile)"
	@echo ""
	@echo "Demo login is defined in .env (Identity__Admin__*). DEVELOPMENT ONLY."
	@echo ""

seed-people: ## Seed ~100 test people via API (requires stack running)
	node scripts/seed-people.mjs
