import { useState } from "react";
import type { FormEvent, ReactNode } from "react";
import { motion } from "motion/react";
import { ArrowLeft, ArrowRight, Lock, Mail, MapPin, Phone, User } from "lucide-react";

type RegisterViewProps = {
  onNavigate: (page: "home" | "login") => void;
  onLogin: (userName: string | null) => void;
};

export function RegisterView({ onNavigate, onLogin }: RegisterViewProps) {
  const [form, setForm] = useState({
    nome: "",
    email: "",
    password: "",
    telemovel: "",
    codigoPostalInput: "",
  });
  const [message, setMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const update = (field: keyof typeof form, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
  };

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage(null);
    setIsSubmitting(true);

    try {
      const response = await fetch("/api/auth/register", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      if (!response.ok) {
        const json = await response.json().catch(() => null);
        setMessage(json?.message ?? json?.title ?? "Nao foi possivel criar a conta.");
        return;
      }

      const json = await response.json().catch(() => null);
      onLogin(json?.userName ?? form.nome);
      onNavigate("home");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="min-h-screen flex flex-col pt-24 pb-12 px-4 relative justify-center"
    >
      <div className="absolute inset-0 z-0">
        <img
          src="https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop"
          alt="Register Background"
          className="w-full h-full object-cover opacity-20 grayscale"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-[#333533] via-[#333533]/80 to-[#333533]/90" />
      </div>

      <div className="flex flex-col items-center justify-center flex-1 z-10 w-full max-w-[1600px] mx-auto">
        <div className="w-full max-w-2xl bg-[#0a0a0a]/60 backdrop-blur-xl border border-white/10 p-8 md:p-10 rounded-[30px] shadow-2xl relative">
          <button
            type="button"
            onClick={() => onNavigate("home")}
            className="flex items-center gap-2 text-white/50 hover:text-yellow-400 font-mono text-xs uppercase tracking-widest transition-colors mb-6 group w-fit"
          >
            <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" /> Voltar
          </button>

          <h1 className="text-4xl md:text-5xl font-bold tracking-tighter uppercase mb-2 leading-none">
            Cria a tua<br /><span className="text-yellow-400">identidade</span>
          </h1>
          <p className="text-white/50 text-sm font-mono tracking-wide mb-7">Junta-te a nos e descobre os melhores eventos.</p>

          <form className="grid grid-cols-1 md:grid-cols-2 gap-4" onSubmit={submit}>
            {message && (
              <div className="md:col-span-2 border border-yellow-400/25 bg-yellow-400/10 text-yellow-100 rounded-xl px-4 py-3 text-xs font-mono">
                {message}
              </div>
            )}

            <Field icon={<User />} label="Nome completo" value={form.nome} onChange={(value) => update("nome", value)} placeholder="O teu nome" />
            <Field icon={<Mail />} label="Email" type="email" value={form.email} onChange={(value) => update("email", value)} placeholder="teu@email.com" />
            <Field icon={<Lock />} label="Password" type="password" value={form.password} onChange={(value) => update("password", value)} placeholder="Cria uma password segura" />
            <Field icon={<Phone />} label="Telemovel" value={form.telemovel} onChange={(value) => update("telemovel", value)} placeholder="912345678" />
            <div className="md:col-span-2">
              <Field icon={<MapPin />} label="Codigo postal" value={form.codigoPostalInput} onChange={(value) => update("codigoPostalInput", value)} placeholder="1000-001" />
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="md:col-span-2 w-full group/btn relative flex items-center justify-center gap-2 bg-yellow-400 text-black px-8 py-4 rounded-xl font-bold uppercase tracking-widest hover:bg-yellow-300 transition-all duration-300 mt-2 shadow-[0_0_20px_rgba(250,204,21,0.15)] hover:shadow-[0_0_30px_rgba(250,204,21,0.3)] disabled:opacity-70"
            >
              <span>{isSubmitting ? "A registar..." : "Registar"}</span>
              <ArrowRight className="w-5 h-5 group-hover/btn:translate-x-1 transition-transform" />
            </button>
          </form>

          <div className="mt-6 text-center border-t border-white/5 pt-6">
            <p className="text-xs text-white/50 font-mono tracking-wide">
              Ja tens conta?
              <button type="button" onClick={() => onNavigate("login")} className="text-yellow-400 hover:text-yellow-300 uppercase tracking-widest font-bold ml-1 transition-colors">
                Entrar
              </button>
            </p>
          </div>
        </div>
      </div>
    </motion.div>
  );
}

function Field({
  icon,
  label,
  value,
  onChange,
  placeholder,
  type = "text",
}: {
  icon: ReactNode;
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder: string;
  type?: string;
}) {
  return (
    <div className="space-y-2">
      <label className="text-[10px] font-mono text-white/50 uppercase tracking-widest ml-1 block">{label}</label>
      <div className="relative group">
        <span className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40 group-focus-within:text-yellow-400 transition-colors [&>svg]:w-4 [&>svg]:h-4">
          {icon}
        </span>
        <input
          type={type}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={placeholder}
          required
          className="w-full bg-white/[0.03] border border-white/10 rounded-xl pl-12 pr-4 py-3.5 text-sm text-white placeholder-white/20 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.05] transition-colors"
        />
      </div>
    </div>
  );
}
