set -e
SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
docker build --pull --no-cache --rm -f $SCRIPT_DIR/Dockerfile -t buildcontainer $SCRIPT_DIR/..
docker run --rm --env-file $SCRIPT_DIR/build.env buildcontainer build/build.csx