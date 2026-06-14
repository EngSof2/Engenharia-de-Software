import { useEffect, useState } from "react";
import { ProfileView } from "./components/ProfileView";
import { EditProfileView } from "./components/EditProfileView";
import { UsersView } from "./components/UsersView";
import { PurchaseHistoryView } from "./components/PurchaseHistoryView";
import { EventsView } from "./components/EventsView";
import { EventDetailsView } from "./components/EventDetailsView";
import { CheckoutView } from "./components/CheckoutView";
import { LoginView } from "./components/LoginView";
import { RegisterView } from "./components/RegisterView";

type Page =
  | "home"
  | "events"
  | "event_details"
  | "checkout"
  | "profile"
  | "edit_profile"
  | "users"
  | "purchase_history"
  | "login"
  | "register";

type SelectedEvent = {
  id: number;
  title: string;
  date: string;
  location: string;
  image?: string | null;
};

declare global {
  interface Window {
    __EH_INITIAL_PAGE__?: Page;
    __EH_INITIAL_TICKET_ID__?: number | null;
  }
}

export default function App() {
  const [page, setPage] = useState<Page>(window.__EH_INITIAL_PAGE__ ?? "home");
  const [selectedEvent, setSelectedEvent] = useState<SelectedEvent | null>(null);
  const [checkoutTicketId, setCheckoutTicketId] = useState<number | null>(
    window.__EH_INITIAL_TICKET_ID__ ?? null
  );

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const nextPage = params.get("page")?.toLowerCase() as Page | undefined;
    const ticketId = Number(params.get("ticketId") || 0);

    if (nextPage) setPage(nextPage);
    if (ticketId > 0) setCheckoutTicketId(ticketId);
  }, []);

  function navigate(nextPage: Page, ticketId?: number | null) {
    setPage(nextPage);

    if (ticketId !== undefined) {
      setCheckoutTicketId(ticketId);
    }

    const params = new URLSearchParams();
    if (nextPage !== "home") params.set("page", nextPage);
    if (ticketId && ticketId > 0) params.set("ticketId", String(ticketId));

    const query = params.toString();
    const url = query ? `/Home/Index?${query}` : "/Home/Index";
    window.history.pushState({ page: nextPage, ticketId: ticketId ?? checkoutTicketId }, "", url);
  }

  const handleEventClick = (event: SelectedEvent) => {
    setSelectedEvent(event);
    navigate("event_details");
  };

  const handleCheckout = (ticketId: number) => {
    navigate("checkout", ticketId);
  };

  if (page === "events") {
    return <EventsView onEventClick={handleEventClick} />;
  }

  if (page === "event_details" && selectedEvent) {
    return (
      <EventDetailsView
        event={selectedEvent}
        onBack={() => navigate("events")}
        onCheckout={handleCheckout}
      />
    );
  }

  if (page === "checkout" && checkoutTicketId) {
    return (
      <CheckoutView
        ticketId={checkoutTicketId}
        onNavigate={navigate}
        onBack={() => (selectedEvent ? navigate("event_details") : navigate("events"))}
      />
    );
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

  if (page === "login") {
    return <LoginView onNavigate={navigate} onLogin={() => navigate("events")} />;
  }

  if (page === "register") {
    return <RegisterView onNavigate={navigate} onLogin={() => navigate("events")} />;
  }

  return (
    <main className="min-h-screen bg-[#333533] text-white">
      <div className="mx-auto max-w-6xl px-10 py-32">
        <h1 className="text-7xl font-black uppercase tracking-tight">Event Horizon</h1>
        <p className="mt-6 max-w-2xl font-mono text-white/60">
          Compra bilhetes, gere o teu perfil e consulta o historico diretamente na SPA.
        </p>
        <div className="mt-10 flex flex-wrap gap-4">
          <button onClick={() => navigate("events")} className="rounded-full border border-white/20 px-6 py-3">
            Eventos
          </button>
          <button onClick={() => navigate("profile")} className="rounded-full bg-yellow-400 px-6 py-3 text-black">
            Perfil
          </button>
          <button onClick={() => navigate("purchase_history")} className="rounded-full border border-white/20 px-6 py-3">
            Historico
          </button>
        </div>
      </div>
    </main>
  );
}
