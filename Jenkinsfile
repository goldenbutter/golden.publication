pipeline {
    agent any

    environment {
        EC2_HOST = "ubuntu@65.2.147.158"
        SSH_KEY = "a6637633-5bab-4a95-a43a-0bf8d077bb35"
    }

    stages {

        stage('Checkout') {
            steps {
                git branch: 'main',
                    credentialsId: 'github-token',
                    url: 'https://github.com/goldenbutter/microchip-interview-private.git'
            }
        }

        stage('Build React Frontend') {
            steps {
                dir('client/publications-client') {
                    sh '''
                        echo "Installing frontend dependencies..."
                        npm ci
                        echo "Building frontend..."
                        npm run build
                    '''
                }
            }
        }

        stage('Build .NET Backend') {
            steps {
                sh '''
                    echo "Restoring .NET dependencies..."
                    dotnet restore Microchip.Interview.Api/Microchip.Interview.Api.csproj

                    echo "Building .NET API..."
                    dotnet publish Microchip.Interview.Api/Microchip.Interview.Api.csproj \
                        -c Release -o Microchip.Interview.Api/publish
                '''
            }
        }

        stage('Build Docker Images') {
            steps {
                sh '''
                    echo "Building API Docker image..."
                    docker build -t publications-api:latest \
                        -f Microchip.Interview.Api/Dockerfile .

                    echo "Building Client Docker image..."
                    docker build -t publications-client:latest \
                        -f client/publications-client/Dockerfile \
                        client/publications-client
                '''
            }
        }

        stage('Deploy to EC2') {
            steps {
                sshagent([SSH_KEY]) {
                    sh '''
                        echo "Deploying to EC2..."

                        ssh -o StrictHostKeyChecking=no $EC2_HOST "
                            cd /var/www/microchip-app &&
                            docker compose down --volumes --remove-orphans &&
                            docker compose build --no-cache &&
                            docker compose up -d
                        "
                    '''
                }
            }
        }

        stage('Restart Services') {
            steps {
                sshagent([SSH_KEY]) {
                    sh '''
                        echo "Restarting Docker services on EC2..."

                        ssh -o StrictHostKeyChecking=no $EC2_HOST "
                            docker compose restart api &&
                            docker compose restart client &&
                            docker compose restart reverse-proxy
                        "
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "Deployment completed successfully!"
        }
        failure {
            echo "Deployment failed. Check logs."
        }
    }
}