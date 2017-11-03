#!/bin/bash
set -ev

echo "${TRAVIS_PULL_REQUEST}"
echo "$TRAVIS_PULL_REQUEST_BRANCH"
echo "$TRAVIS_BRANCH"

if [ "${TRAVIS_PULL_REQUEST}" = "false" ] && [ "$TRAVIS_BRANCH" = "master" ]; then
    echo "Publishing Brthor.Http $(printf %05d $TRAVIS_BUILD_NUMBER)"

    dotnet pack -c Release --version-suffix $(printf %05d $TRAVIS_BUILD_NUMBER)
    dotnet nuget push ./bin/Release/Brthor.Http.*.nupkg -s https://www.myget.org/F/thor/api/v2/package -k $MYGET_KEY
fi
