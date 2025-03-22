# TCFiap Consumer Update Contact API

Este projeto é um serviço .NET que consome mensagens para atualização de contatos através do MassTransit e RabbitMQ. Quando uma mensagem do tipo `UpdateContactMessage` é recebida, o serviço busca o contato no banco de dados, atualiza os dados (nome, telefone, email).

## Sumário

- [Visão Geral](#visão-geral)
- [Recursos](#recursos)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Como Executar o Projeto](#como-executar-o-projeto)
  - [Executando Localmente](#executando-localmente)
  - [Utilizando Docker](#utilizando-docker)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Testes](#testes)
- [Configuração](#configuração)
- [Licença](#licença)

## Visão Geral

O TCFiap Consumer Update Contact é responsável por processar mensagens de atualização de contatos, integrando-se com:

- **MassTransit** para gerenciamento e consumo de mensagens via RabbitMQ;
- **TechChallenge.SDK** para operações de persistência e funcionalidades específicas do domínio;
- **Worker Service** para manter a execução contínua do consumo de mensagens.

Ao receber uma mensagem `UpdateContactMessage`, o serviço localiza o contato, atualiza seus dados e registra o sucesso ou eventuais falhas no processo através de logs.

## Recursos

- **Consumo de Mensagens:** Processa mensagens para atualização de contatos.
- **Integração com RabbitMQ:** Configuração do endpoint `update-contact-queue` para receber mensagens.
- **Atualização de Contatos:** Mapeia os dados da mensagem para atualizar os atributos do contato (nome, telefone, email).
- **Logs Detalhados:** Registra informações de início, sucesso e problemas durante o processamento.
- **Testes:** Inclui testes unitários e de integração utilizando NUnit e Moq.

## Tecnologias Utilizadas

- .NET 8
- MassTransit
- RabbitMQ
- Docker
- NUnit
- Moq
- TechChallenge.SDK

## Pré-requisitos

- .NET SDK 8.0
- RabbitMQ (disponível localmente ou via container)
- Variável de ambiente para a connection string:
  - `CONNECTION_DATABASE`: string de conexão com o banco de dados.
- Variável de ambiente para o host do RabbitMQ:
  - `RABBITMQ_HOST` (opcional, padrão: `localhost`).
- Docker, se desejar executar via container.

## Como Executar o Projeto

### Executando Localmente

Clone o repositório:

```
git clone https://seurepositorio.com/TCFiapConsumerUpdateContact.git
cd TCFiapConsumerUpdateContact
```

Configure as variáveis de ambiente:

- `CONNECTION_DATABASE`: string de conexão com o banco de dados.
- `RABBITMQ_HOST`: (opcional) host do RabbitMQ, se necessário.

Restaure os pacotes e compile o projeto:

```
dotnet restore
dotnet build
```    

Execute a aplicação:

```
dotnet run --project TCFiapConsumerUpdateContact.API
```   

### Utilizando Docker

O projeto contém um Dockerfile que utiliza um processo de build multi-stage.

Defina o argumento de senha do NuGet (caso necessário) e construa a imagem Docker:

```
docker build --build-arg ARG_SECRET_NUGET_PACKAGES=SuaSenhaAqui -t tcfiap-consumer-update-contact .
```

Execute o container, definindo as variáveis de ambiente necessárias:

```
docker run -d -p 8080:8080 --env CONNECTION_DATABASE="SuaConnectionString" --env RABBITMQ_HOST="SeuHostRabbitMQ" tcfiap-consumer-update-contact
```

## Estrutura do Projeto

- **TCFiapConsumerUpdateContact.API:** Projeto principal que configura:
  - O MassTransit com o consumidor `UpdateContactConsumer` e o endpoint `update-contact-queue`.
  - O registro do TechChallenge.SDK com a connection string.
  - O Worker Service para gerenciamento do ciclo de vida do bus.
- **Worker.cs:** Serviço de background que inicia e finaliza o bus do MassTransit.
- **UpdateContactConsumer.cs:** Consumidor que processa as mensagens `UpdateContactMessage` para atualizar os contatos.
- **Testes:**
  - **TCFiapConsumerUpdateContact.Tests.IntegrationTests:** Testes de integração utilizando o InMemoryTestHarness do MassTransit.
  - **TCFiapConsumerUpdateContact.Tests.UnitTests:** Testes unitários que validam a lógica do consumidor e o registro de logs.
- **Dockerfile:** Configuração para build multi-stage e publicação da aplicação.

## Testes

O projeto inclui testes para garantir o correto funcionamento do serviço:

- **Testes Unitários:** Validam a lógica de atualização de contatos e o comportamento do registro de logs.
- **Testes de Integração:** Verificam o fluxo completo de consumo de mensagens utilizando o MassTransit In-Memory Test Harness.


## Configuração

**MassTransit & RabbitMQ:**

- **Endpoint de Recebimento:** `update-contact-queue`

**SDK:**

- O módulo do TechChallenge.SDK é registrado através do método `RegisterSdkModule`, utilizando a connection string do banco de dados.

**Variáveis de Ambiente:**

- `CONNECTION_DATABASE`: String de conexão com o banco de dados.
- `RABBITMQ_HOST`: Host do RabbitMQ (padrão: `localhost`).
