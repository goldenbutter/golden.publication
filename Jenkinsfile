pipeline {
    agent any

    environment {
        EC2_HOST = "ubuntu@13.205.157.132"
        SSH_KEY = "a6637633-5bab-4a95-a43a-0bf8d077bb35"
    }

    stages {

        stage('Checkout') {
            steps {
                echo "Checking out code..."
                git branch: 'dev',
                    credentialsId: 'github-token',
                    url: 'https://github.com/goldenbutter/microchip-interview-private.git'
            }
        }

        stage('Copy Code to EC2') {
            steps {
                echo "Copying code to EC2..."
                sshagent([env.SSH_KEY]) {
                    sh '''
                        rsync -avz --delete \
                        ./ $EC2_HOST:/home/ubuntu/project/microchip-interview-private/
                    '''
                }
            }
        }

        stage('Deploy on EC2') {
            steps {
                echo "Deploying on EC2..."
                sshagent([SSH_KEY]) {
                    sh '''
                        ssh -o StrictHostKeyChecking=no $EC2_HOST "
                            cd /home/ubuntu/project/microchip-interview-private &&
                            docker-compose down &&
                            docker-compose up -d --build
                        "
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "🎉 Deployment completed successfully!"
        }
        failure {
            echo "❌ Deployment failed. Check logs."
        }
    }
}