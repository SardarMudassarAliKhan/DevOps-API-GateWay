pipeline {
    agent any
    
    options {
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '5'))
    }   
    
    stages {
        stage('Checkout Source') {
            steps {
                checkout scm
            }
        }

        stage('Build') {
            steps {
                script {
                    echo 'Cleaning up old production images...'
                    sh 'docker rmi devops-api-gateway:latest || true'

                    echo 'Building the new Docker image...'
                    // Target the API Gateway folder and Dockerfile
                    sh 'docker build --no-cache -t devops-api-gateway:latest -f DevOps-API-GateWay/Dockerfile .'
                }
            }
        }

        stage('Push and Deploy') {
            steps {
                script {
                    echo 'Ensuring isolated persistent data protection keys directory exists on host...'
                    sh 'mkdir -p /home/ubuntu/devops-api-gateway/aspnet-keys'
                    
                    echo 'Stopping old active container if it exists...'
                    sh 'docker stop devops-api-gateway-web || true'
                    sh 'docker rm devops-api-gateway-web || true'
                    
                    echo 'Checking availability of Loki logging driver...'
                    def lokiAvailable = sh(script: "docker plugin ls --format '{{.Name}}' | grep -q 'loki'", returnStatus: true) == 0
                    
                    def loggingOpts = lokiAvailable ? 
                        '--log-driver=loki --log-opt loki-url="http://127.0.0.1:3100/loki/api/v1/push" --log-opt loki-external-labels="container_name={{.Name}}"' : 
                        '--log-driver=json-file --log-opt max-size=10m --log-opt max-file=3'

                    echo "Running new container with logging strategy: ${lokiAvailable ? 'Loki' : 'JSON File Fallback'}..."
                    
                    // Run the API Gateway
                    // Port 5000 on the host maps to port 8080 inside the container
                    sh """
                        docker run -d --restart always --name devops-api-gateway-web \
                        ${loggingOpts} \
                        --env "ASPNETCORE_ENVIRONMENT=Production" \
                        --env "ASPNETCORE_URLS=http://0.0.0.0:8080" \
                        -v /home/ubuntu/devops-api-gateway/aspnet-keys:/home/app/.aspnet/DataProtection-Keys \
                        --network mudassar -p 8083:8080 devops-api-gateway:latest
                    """
                }
            }
        }

        stage('Cleanup') {
            steps {
                script {
                    echo 'Cleaning up dangling images...'
                    sh 'docker image prune -f'
                }
            }
        }
    }
}