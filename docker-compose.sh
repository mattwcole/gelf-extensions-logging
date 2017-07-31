#!/bin/bash
set -e

# Wrap docker-compose and return a non-zero exit code if any containers failed.

docker-compose "$@"

exit $(docker-compose -f docker-compose.ci.build.yml ps -q | tr -d '[:space:]' |
  xargs docker inspect -f '{{ .State.ExitCode }}' | grep -v 0 | wc -l | tr -d '[:space:]')
