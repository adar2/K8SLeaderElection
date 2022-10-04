# K8SLeaderElection
This repository provides a simple example of how to use the C# client to conduct K8s leader election.

# How to run

## Docker
First you will need a docker image that contains the application.

To build the image you will need Docker, which you can download [here](https://www.docker.com/) if you don't already have it.

Next, you will need to run the following command:

```docker build -t k8s-leader-election-image <PATH_TO_OUR_REPO_FOLDER>```

`k8s-leader-election-image` will appear in your Docker images after the build has finished successfully.

## Minikube
Now, we will need a cluster to run some isntances of the image we have just created.

I use minikube, so go ahead and get it [here](https://minikube.sigs.k8s.io/docs/start/) if you don't already have it.

In order to start minikube run:
`minikube start`.

Next, we will need to load the image that we've created before to the minikube docker registry, to do so, run the following command:
`minikube image load k8s-leader-election-image:latest`

To work with our k8s cluster we will need a cli tool called `kubectl`, you can get it [here](https://kubernetes.io/docs/tasks/tools/).

We can now create the K8s pods that will run our application.

Go ahead and run `kubectl apply -f test-pod.yml`.

Three pods will be created, you can list the pods using `kubectl get pod`, and watch the application output using `kubectl logs -f <POD_NAME>`.





