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
