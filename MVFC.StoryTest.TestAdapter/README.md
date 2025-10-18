# MVFC.StoryTest.TestAdapter

Adaptador de integração para o framework MVFC.StoryTest, facilitando a execução e o reconhecimento de cenários de teste (Story Tests) em ambientes e ferramentas de automação de testes .NET.

## Recursos

- Permite a execução de cenários definidos com MVFC.StoryTest em runners e pipelines de teste .NET.
- Integração transparente com o ecossistema de testes do .NET 9+.
- Suporte a cenários, steps e hooks definidos no padrão BDD.

## Instalação

Adicione a referência ao pacote no seu projeto de testes:

```
dotnet add package MVFC.StoryTest.TestAdapter
```

## Como funciona

O TestAdapter detecta e executa cenários definidos com o MVFC.StoryTest, permitindo que eles sejam reconhecidos por ferramentas de execução de testes, como Visual Studio Test Explorer, dotnet test, Azure DevOps, entre outros.

## Exemplo de uso

1. Defina seus cenários normalmente usando o MVFC.StoryTest.
2. Adicione o pacote `MVFC.StoryTest.TestAdapter` ao seu projeto de testes.
3. Execute os testes utilizando sua ferramenta preferida:


```
dotnet test
```

Os cenários serão listados e executados como testes convencionais.

## Requisitos

- .NET 9 ou superior
- Projeto com cenários definidos usando MVFC.StoryTest

## Licença

Este projeto é distribuído sob a licença MIT.