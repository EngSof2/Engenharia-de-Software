export type Profile = {
  id: number;
  userName: string;
  nome: string;
  email: string;
  telemovel?: string;
  tipoUti: number;
  perfil: string;
};

export type UserRow = {
  id: number;
  nome: string;
  email: string;
  telemovel?: string;
  perfil: string;
  tipoUti: number;
};

export type Purchase = {
  recibo: string;
  idRecibo: number;
  evento: string;
  nomeBilhete: string;
  tipoBilhete: string;
  acesso: string;
  metodoPagamento: string;
  data: string;
  valor: number;
};

export type PaymentMethod = {
  idTipoPagamento: number;
  nome: string;
};

export type CheckoutData = {
  idBilheteEvento: number;
  idEvento: number;
  nomeEvento: string;
  dataEvento: string | null;
  horaEvento: string | null;
  localEvento: string | null;
  nomeBilhete: string;
  tipoBilhete: string;
  descricaoAcesso: string;
  preco: number;
  quantidadeDisponivel: number;
  nomeComprador: string;
  email: string;
  telemovel: string;
  morada: string;
  tiposPagamento: PaymentMethod[];
};

export type CheckoutPayload = {
  idBilheteEvento: number;
  nomeComprador: string;
  email: string;
  telemovel: string;
  morada: string;
  idTipoPagamento: number;
  numeroCartao?: string;
  nomeTitular?: string;
  validadeCartao?: string;
  cvv?: string;
  emailPaypal?: string;
};

async function request<T>(url: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(url, {
    credentials: "include",
    headers: { "Content-Type": "application/json", ...options.headers },
    ...options
  });

  if (!response.ok) {
    let message = "Nao foi possivel concluir o pedido.";
    try {
      const body = await response.json();
      message = body.message || message;
    } catch {
      // Mantem a mensagem generica.
    }
    throw new Error(message);
  }

  return response.json() as Promise<T>;
}

export const api = {
  getProfile: () => request<Profile>("/api/auth/me"),

  updateProfile: (profile: Pick<Profile, "nome" | "email" | "telemovel">) =>
    request<Profile>("/api/auth/me", {
      method: "PUT",
      body: JSON.stringify(profile)
    }),

  getUsers: () => request<UserRow[]>("/api/utilizadores"),

  getPurchaseHistory: () => request<Purchase[]>("/api/bilhetes/historico"),

  getCheckout: (id: number) => request<CheckoutData>(`/api/bilhetes/checkout/${id}`),

  purchaseTicket: (payload: CheckoutPayload) =>
    request<{ message: string }>("/api/bilhetes/checkout", {
      method: "POST",
      body: JSON.stringify(payload)
    }),

  deleteEvent: (id: number) =>
    request(`/api/eventos/${id}`, {
      method: "DELETE"
    }),

  updateEvent: (id: number, data: any) =>
    request(`/api/eventos/${id}`, {
      method: "PUT",
      body: JSON.stringify(data)
    })
};
