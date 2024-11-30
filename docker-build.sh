#!/bin/bash

set -ex

IMAGE_NAME="nicolehm1/odataapigen"
TAG=arm

REGISTRY="docker.io"

docker build -t ${REGISTRY}/${IMAGE_NAME}:${TAG} -t ${REGISTRY}/${IMAGE_NAME}:latest .
docker push ${REGISTRY}/${IMAGE_NAME}:${TAG}
docker push ${REGISTRY}/${IMAGE_NAME}:latest
