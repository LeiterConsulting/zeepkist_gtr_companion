#!/usr/bin/env bash

set -euo pipefail

repository_root="$(git rev-parse --show-toplevel)"
cd "$repository_root"

blocked_path_pattern='(^|/)(ios|android|(private-)?ios-app|(private-)?android-app|\.private|\.local|\.scratch|dev-tests|local-tests|notes|research|scratch|conversations|transcripts|prompts|secrets|captures|dumps)(/|$)'
blocked_file_pattern='(\.xcodeproj(/|$)|\.xcworkspace(/|$)|\.xcuserstate$|\.mobileprovision$|\.keystore$|\.jks$|(^|/)(key|local)\.properties$|(^|/)google-services\.json$|(^|/)google-service-info\.plist$|(^|/)\.env($|\.)|\.p12$|\.pfx$|\.pem$|\.key$|secrets[^/]*\.json$)'

failed=0

while IFS= read -r path; do
    normalized_path="$(printf '%s' "$path" | tr '[:upper:]' '[:lower:]')"

    if [[ "$normalized_path" =~ $blocked_path_pattern ]] ||
       [[ "$normalized_path" =~ $blocked_file_pattern ]]; then
        printf 'Blocked public repository path: %s\n' "$path" >&2
        failed=1
    fi
done < <(git ls-files --cached --others --exclude-standard)

if [[ "$failed" -ne 0 ]]; then
    printf 'Public repository boundary check failed.\n' >&2
    exit 1
fi

printf 'Public repository boundary check passed.\n'
