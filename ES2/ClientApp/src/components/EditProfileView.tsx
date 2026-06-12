import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";

type Props = {
  onNavigate: (page: "profile") => void;
};

export function EditProfileView({ onNavigate }: Props) {
  const [form, setForm] = useState({ nome: "", email: "", telemovel: "" });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.getProfile().then((profile) =>
      setForm({
        nome: profile.nome,
        email: profile.email,
        telemovel: profile.telemovel || ""
      })
    );
  }, []);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      await api.updateProfile(form);
      setMessage("Utilizador atualizado com sucesso!");
      setTimeout(() => onNavigate("profile"), 1200);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Nao foi possivel guardar o perfil.");
    }
  }

  return (
    <main className="relative min-h-screen overflow-hidden px-6 py-24 text-white">
      <div className="absolute inset-0 bg-[#333533]" />
      <div className="relative z-10 mx-auto max-w-[1120px] rounded-[30px] border border-white/10 bg-[#0a0a0a]/72 p-12 shadow-2xl backdrop-blur-2xl">
        <button
          onClick={() => onNavigate("profile")}
          className="mb-8 font-mono text-xs font-bold uppercase tracking-[0.28em] text-white/80 hover:text-yellow-400"
        >
          {"<-"} Voltar ao perfil
        </button>

        <div className="mb-6 font-mono text-xs font-black uppercase tracking-[0.38em] text-yellow-400">
          Area pessoal
        </div>
        <h1 className="mb-6 text-[76px] font-black uppercase leading-none tracking-tight">Editar Perfil</h1>
        <p className="mb-8 font-mono text-lg text-white/75">Atualiza os dados que aparecem associados a tua conta.</p>

        {message && <div className="mb-8 rounded-2xl border border-yellow-400/35 bg-yellow-400/15 px-5 py-4 font-mono">{message}</div>}
        {error && <div className="mb-8 rounded-2xl border border-red-400/35 bg-red-400/10 px-5 py-4 font-mono">{error}</div>}

        <form onSubmit={submit} className="space-y-7">
          <Field label="Nome" value={form.nome} onChange={(nome) => setForm({ ...form, nome })} />
          <Field label="Email" type="email" value={form.email} onChange={(email) => setForm({ ...form, email })} />
          <Field label="Telemovel" value={form.telemovel} onChange={(telemovel) => setForm({ ...form, telemovel })} />

          <div className="flex flex-wrap gap-4 pt-6">
            <button className="rounded-full bg-yellow-400 px-9 py-4 font-black uppercase tracking-[0.18em] text-black hover:bg-yellow-300">
              Guardar
            </button>
            <button
              type="button"
              onClick={() => onNavigate("profile")}
              className="rounded-full border border-white/20 bg-white/5 px-9 py-4 font-black uppercase tracking-[0.18em] text-white hover:bg-white/10"
            >
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </main>
  );
}

function Field(props: { label: string; value: string; type?: string; onChange: (value: string) => void }) {
  return (
    <label className="block space-y-3">
      <span className="block font-mono text-xs font-black uppercase tracking-[0.18em] text-white/80">{props.label}</span>
      <input
        type={props.type || "text"}
        value={props.value}
        onChange={(event) => props.onChange(event.target.value)}
        className="w-full rounded-2xl border border-white/10 bg-white/[0.07] px-5 py-4 font-mono text-lg text-white outline-none focus:border-yellow-400"
      />
    </label>
  );
}
