# Aplicativo de Censo

O Aplicativo do Censo foi desenvolvido com base na arquitetura de microsserviços. A aplicação consiste de 3 microsserviços, a saber:

 - **People**: Responsável pelo CRUD dos dados de cada cidadão. Este microsserviço utiliza-se do MongoDB para manter uma coleção de documentos com os dados básicos de cada pessoa.
 - **Statistics**: Responsável por manter contadores com os dados estatísticos agrupados. Este microsserviço mantém um conjunto de coleções (também no MongoDB) com os dados agrupados por categorias baseadas nas características de cada pessoa.
 - **FamilyTree**: Responsável por manter as árvores genealógicas de cada cidadão. As árvores são montadas e atualizadas à medida em que novas pessoas são cadastradas no sistema. Este microsserviço utiliza um banco de dados de grafos (Neo4J) para permitir a consulta em diversos níveis na árvore genealógica. 
 
 Os microsserviços se comunicam utilizando o um Event Bus baseado em RabbitMQ.

![Arquitetura](https://i.ibb.co/fDpr1Tr/arquitetura.jpg)

### Execução

A aplicação está baseda em Containers e pode ser executada com o seguinte comando (depende de um ambiente contendo git CLI, docker e docker-compose)

	git clone https://github.com/hllustosa/CensoDemografico
	cd CensoDemografico/ 
	docker-compose up

A aplicação estará acessível em http://localhost:8080/    

### Pipeline e Link para o AKS
[![Build Status](https://dev.azure.com/hermanolustosa/Census/_apis/build/status/hllustosa.CensoDemografico?branchName=master)](https://dev.azure.com/hermanolustosa/Census/_build/latest?definitionId=1&branchName=master)

A aplicação está integrada ao [Azure DevOps](https://dev.azure.com/hermanolustosa/Census) e possui um deploy baseado em kubernetes na AKS. [Link aqui](http://40.119.48.137/).

Um vídeo demonstrativo também está disponível [neste link](https://youtu.be/HneqbjmSPPQ).
