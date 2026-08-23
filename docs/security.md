# Dependency and security maintenance

## How dependencies are maintained

- Central package versions in `Directory.Packages.props` (+ microservice mirror where required)
- GitHub Dependabot for NuGet and npm (`.github/dependabot.yml`)
- Prefer LTS runtimes (currently .NET 8)

## How security issues are detected

- GitHub Actions `security.yml`: dependency review, CodeQL, Trivy image scan
- CI builds all service images including Identity
- JWT signing key is required at startup (no silent weak fallback)
- Compose secrets come from `.env` (see `.env.example`); demo values are **DEVELOPMENT ONLY**
- Local Mongo runs as an unauthenticated single-node replica set so transactional outbox works without keyFile complexity; production should use auth + keyFile / TLS
