# Public Repository Policy

This repository is intentionally limited to material that can be published as
part of the Zeepkist GTR Companion plugin project.

## Product boundary

The official iOS companion app is a separate, proprietary paid product
distributed only through the Apple App Store. Its source code, product
features, project files, assets, store metadata, and signing material do not
belong in this repository.

Any future official Android companion app will follow the same separate paid
product boundary. The public protocol remains documented here so the plugin
and companion products can evolve against a clear, versioned contract.

## Allowed

- Zeepkist plugin source code
- Build and release configuration
- Public protocol specifications
- Platform-neutral schemas, fixtures, and interoperability documentation
- Public-facing documentation and installation instructions
- Contribution, security, and support information
- Assets that are licensed for public redistribution

## Not allowed

- Proprietary iOS or Android app source, feature code, or project files
- Mobile app product assets, store metadata, signing files, or credentials
- Credentials, authentication tickets, access tokens, or signing material
- Personal data, captured gameplay sessions, or local configuration
- Private planning notes, research dumps, prompts, or conversation transcripts
- Scratch code, local experiments, or developer-only test artifacts
- Third-party game or plugin files that cannot be redistributed

## Before committing

1. Run `./scripts/check-public-boundary.sh`.
2. Review every staged path with `git diff --cached --name-status`.
3. Review the staged content with `git diff --cached`.
4. Confirm that generated binaries and local runtime data are absent.
5. Confirm that every third-party asset has a redistribution-compatible
   license and attribution.

The repository guard catches common mistakes, but it is not a substitute for
reviewing the exact content being published.
