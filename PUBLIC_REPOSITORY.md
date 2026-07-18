# Public Repository Policy

This repository is intentionally limited to material that can be published as
part of the Zeepkist GTR Companion plugin project.

## Allowed

- Zeepkist plugin source code
- Build and release configuration
- Public protocol specifications
- Public-facing documentation and installation instructions
- Contribution, security, and support information
- Assets that are licensed for public redistribution

## Not allowed

- Companion app source code or Xcode projects
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
