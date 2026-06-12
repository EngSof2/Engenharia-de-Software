# Event Horizon SPA Source

Esta pasta contem uma versao legivel da interface SPA em React/TSX.

No projeto ASP.NET, a SPA que o browser executa esta em:

`ES2/wwwroot/event-horizon/assets/event-horizon.js`

Esse ficheiro e um bundle gerado/compilado e por isso nao e pratico de ler. Os ficheiros nesta pasta servem como fonte organizada para explicar a UI:

- `src/App.tsx`: exemplo de navegacao SPA por estado.
- `src/components/EventsView.tsx`: listagem/pesquisa de eventos.
- `src/components/EventDetailsView.tsx`: detalhes de um evento, atividades e bilhetes.
- `src/components/CreateEventView.tsx`: criacao de eventos.
- `src/components/LoginView.tsx`: login em SPA.
- `src/components/RegisterView.tsx`: registo em SPA.
- `src/components/ProfileView.tsx`: detalhes do perfil.
- `src/components/EditProfileView.tsx`: edicao do perfil.
- `src/components/UsersView.tsx`: gestao de utilizadores.
- `src/components/EditUserView.tsx`: edicao de utilizadores pelo admin.
- `src/components/PurchaseHistoryView.tsx`: historico de compras.
- `src/components/InteractiveTicket.tsx`: componente visual de bilhete usado nos detalhes.
- `src/api.ts`: chamadas HTTP usadas pela UI.

Fluxo:

```text
ClientApp/src/*.tsx
        -> build React/Vite
wwwroot/event-horizon/assets/event-horizon.js
        -> carregado por Views/Home/Index.cshtml
        -> browser mostra a SPA
```

Nota: esta pasta foi adicionada como fonte legivel/documentavel. O projeto atual ainda serve o bundle ja existente em `wwwroot/event-horizon`.

Alguns componentes vieram diretamente da base feita no AI Studio e podem nao estar 100% iguais ao estado final do projeto ASP.NET. As partes que foram integradas/adaptadas no projeto final estao representadas nos componentes principais e no bundle servido em `wwwroot/event-horizon/assets/event-horizon.js`.
