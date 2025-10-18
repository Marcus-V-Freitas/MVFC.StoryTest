# MVFC.StoryTest.TestAdapter

Adaptador de integra��o para o framework MVFC.StoryTest, facilitando a execu��o e o reconhecimento de cen�rios de teste (Story Tests) em ambientes e ferramentas de automa��o de testes .NET.

## Recursos

- Permite a execu��o de cen�rios definidos com MVFC.StoryTest em runners e pipelines de teste .NET.
- Integra��o transparente com o ecossistema de testes do .NET 9+.
- Suporte a cen�rios, steps e hooks definidos no padr�o BDD.

## Instala��o

Adicione a refer�ncia ao pacote no seu projeto de testes:

```
dotnet add package MVFC.StoryTest.TestAdapter
```

## Como funciona

O TestAdapter detecta e executa cen�rios definidos com o MVFC.StoryTest, permitindo que eles sejam reconhecidos por ferramentas de execu��o de testes, como Visual Studio Test Explorer, dotnet test, Azure DevOps, entre outros.

## Exemplo de uso

1. Defina seus cen�rios normalmente usando o MVFC.StoryTest.
2. Adicione o pacote `MVFC.StoryTest.TestAdapter` ao seu projeto de testes.
3. Execute os testes utilizando sua ferramenta preferida:


```
dotnet test
```

Os cen�rios ser�o listados e executados como testes convencionais.

## Requisitos

- .NET 9 ou superior
- Projeto com cen�rios definidos usando MVFC.StoryTest

## Licen�a

Este projeto � distribu�do sob a licen�a MIT.