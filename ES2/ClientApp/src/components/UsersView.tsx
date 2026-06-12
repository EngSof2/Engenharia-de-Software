import { useEffect, useMemo, useState } from "react";
import { api, UserRow } from "../api";

export function UsersView() {
  const [users, setUsers] = useState<UserRow[]>([]);
  const [search, setSearch] = useState("");
  const [selectedId, setSelectedId] = useState<number | null>(null);

  useEffect(() => {
    api.getUsers().then(setUsers);
  }, []);

  const visibleUsers = useMemo(() => {
    const query = search.toLowerCase();
    if (!query) return users;
    return users.filter((user) =>
      [String(user.id), user.nome, user.email, user.telemovel || "", user.perfil].some((value) =>
        value.toLowerCase().includes(query)
      )
    );
  }, [users, search]);

  return (
    <main className="min-h-screen bg-[#333533] px-4 py-32 text-white">
      <div className="mx-auto max-w-[1400px]">
        <h1 className="mb-2 text-5xl font-bold uppercase tracking-tighter">
          Gestao de <span className="text-yellow-400">Utilizadores</span>
        </h1>
        <p className="mb-12 font-mono text-sm text-white/50">Visualiza, gere e administra todos os perfis da plataforma.</p>

        <div className="mb-8 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Pesquisar por ID, nome, email..."
            className="w-full max-w-md rounded-full border border-white/10 bg-black/20 px-6 py-3.5 font-mono text-sm outline-none focus:border-yellow-400"
          />
          <button
            disabled={!selectedId}
            className="rounded-full bg-yellow-400 px-6 py-3.5 text-xs font-bold uppercase tracking-widest text-black disabled:bg-white/5 disabled:text-white/20"
          >
            Editar
          </button>
        </div>

        <div className="overflow-hidden rounded-[30px] border border-white/10 bg-[#0a0a0a]/60 backdrop-blur-2xl">
          <table className="w-full text-left">
            <thead className="bg-white/[0.02] text-xs uppercase tracking-widest text-white/50">
              <tr>
                <th className="p-6">ID</th>
                <th className="p-6">Nome</th>
                <th className="p-6">Email</th>
                <th className="p-6">Telemovel</th>
                <th className="p-6">Perfil</th>
              </tr>
            </thead>
            <tbody>
              {visibleUsers.map((user) => {
                const selected = selectedId === user.id;
                return (
                  <tr
                    key={user.id}
                    onClick={() => setSelectedId(selected ? null : user.id)}
                    className={`cursor-pointer border-t border-white/5 ${selected ? "bg-yellow-400/15 text-yellow-400" : "hover:bg-white/[0.03]"}`}
                  >
                    <td className="p-6 font-mono">{user.id}</td>
                    <td className="p-6 font-medium">{user.nome}</td>
                    <td className="p-6 text-sm">{user.email}</td>
                    <td className="p-6 font-mono text-sm">{user.telemovel || "-"}</td>
                    <td className="p-6">
                      <span className="rounded-full border border-white/10 px-3 py-1 text-[10px] font-bold uppercase tracking-widest">
                        {user.perfil}
                      </span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </main>
  );
}
