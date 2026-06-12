import { useState } from "react";
import { ProfileView } from "./components/ProfileView";
import { EditProfileView } from "./components/EditProfileView";
import { UsersView } from "./components/UsersView";
import { PurchaseHistoryView } from "./components/PurchaseHistoryView";

type Page = "home" | "events" | "profile" | "edit_profile" | "users" | "purchase_history";

export function App() {
  const [page, setPage] = useState<Page>("home");

  function navigate(nextPage: Page) {
    setPage(nextPage);
    const url = nextPage === "home" ? "/Home/Index" : `/Home/Index?page=${nextPage}`;
    window.history.pushState({ page: nextPage }, "", url);
  }

  if (page === "profile") {
    return <ProfileView onNavigate={navigate} />;
  }

  if (page === "edit_profile") {
    return <EditProfileView onNavigate={navigate} />;
  }

  if (page === "users") {
    return <UsersView />;
  }

  if (page === "purchase_history") {
    return <PurchaseHistoryView onNavigate={navigate} />;
  }

  return (
    <main className="min-h-screen bg-[#333533] text-white">
      <div className="mx-auto max-w-6xl px-10 py-32">
        <h1 className="text-7xl font-black uppercase tracking-tight">Event Horizon</h1>
        <p className="mt-6 max-w-2xl font-mono text-white/60">
          Esta e a entrada da SPA. No bundle real, esta pagina inclui tambem o carrossel,
          eventos, login, registo e detalhes de evento.
        </p>
        <div className="mt-10 flex gap-4">
          <button onClick={() => navigate("events")} className="rounded-full border border-white/20 px-6 py-3">
            Eventos
          </button>
          <button onClick={() => navigate("profile")} className="rounded-full bg-yellow-400 px-6 py-3 text-black">
            Perfil
          </button>
        </div>
      </div>
    </main>
  );
}
