# TimingÔnibus

O **TimingÔnibus** é um sistema que tem como objetivo facilitar o acompanhamento do tempo de chegada dos ônibus em um determinado ponto.

A aplicação utiliza dados da [API] de localização em tempo real dos ônibus de Belo Horizonte.

---

## Funcionalidades

O sistema permite dois fluxos de interação:

- **Seleção por linha:** O usuário seleciona uma linha de ônibus e é exibido no mapa todos os pontos atendidos por essa linha.
- **Seleção por ponto:** O usuário seleciona um ponto no mapa e é exibido a ele todas as linhas que atendem aquele ponto.

A partir disso, o sistema irá consultar a [API] de localização em tempo real e irá exibir uma notificação ao usuário mostrando:

- O **tempo estimado** de chegada do próximo ônibus.
- Se um ônibus **acabou de passar** pelo ponto.

---

## Tecnologias Utilizadas

- **C# (.NET)** — Desenvolvimento e validação da lógica principal e protótipo da interface inicial.

---

## Futuras Implementações

Atualmente, o projeto está em **fase de validação da lógica** e da integração com a [API].

As próximas etapas incluem:

- Desenvolvimento de um aplicativo para Android utilizando **Kotlin**.
- Aplicativo para **smartwatches Amazfit**.

---

## Contribuições

Contribuições são sempre bem-vindas!
Basta abrir uma issue ou enviar um pull request com melhorias ou novas ideias.


[API]: https://dados.pbh.gov.br/dataset/tempo_real_onibus_-_coordenada
