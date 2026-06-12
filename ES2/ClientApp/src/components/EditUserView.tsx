import { useState } from "react";
import { motion } from "motion/react";
import { User as UserIcon, Check, ChevronDown } from "lucide-react";

interface User {
  id: number;
  name: string;
  email: string;
  phone: string;
  profile: string;
}

interface EditUserViewProps {
  user: User;
  onCancel: () => void;
  onSave: (user: User) => void;
}

export function EditUserView({ user, onCancel, onSave }: EditUserViewProps) {
  const [formData, setFormData] = useState({
    name: user.name,
    email: user.email,
    phone: user.phone || "",
    profile: user.profile
  });

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="w-full min-h-screen relative flex flex-col pt-32 pb-32 px-4"
    >
      {/* Background Image Setup */}
      <div className="absolute inset-0 z-0 overflow-hidden">
        <img 
          src="https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=2000&auto=format&fit=crop" 
          alt="Edit User Background" 
          className="w-full h-full object-cover brightness-[0.3] blur-[6px] scale-105" 
        />
        <div className="absolute inset-0 bg-black/40" />
      </div>

      <div className="max-w-2xl mx-auto w-full relative z-10">
        <div className="w-full bg-black/20 backdrop-blur-2xl border border-white/10 p-10 md:p-12 rounded-[30px] relative shadow-2xl overflow-hidden">
          
          <div className="flex items-center gap-3 mb-10 border-b border-white/10 pb-6">
            <UserIcon className="w-5 h-5 text-white/80" />
            <h2 className="text-2xl font-bold tracking-tighter text-white">{user.name}</h2>
            <span className="bg-white/10 text-white/80 text-xs font-mono px-2 py-1 rounded ml-2">
              #{user.id}
            </span>
          </div>

          <div className="space-y-8">
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Nome</label>
              <input 
                type="text" 
                value={formData.name}
                onChange={(e) => setFormData({...formData, name: e.target.value})}
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>

            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Email</label>
              <input 
                type="email" 
                value={formData.email}
                onChange={(e) => setFormData({...formData, email: e.target.value})}
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>

            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Telemovel</label>
              <input 
                type="text" 
                value={formData.phone}
                onChange={(e) => setFormData({...formData, phone: e.target.value})}
                className="w-full bg-transparent border border-white/20 rounded-xl px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>

            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Perfil / Permissões</label>
              <div className="relative group border border-white/20 rounded-xl focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors">
                <select 
                  value={formData.profile}
                  onChange={(e) => setFormData({...formData, profile: e.target.value})}
                  className="w-full bg-transparent rounded-xl pl-6 pr-12 py-4 text-sm text-white focus:outline-none appearance-none cursor-pointer relative z-10 transition-colors"
                >
                  <option value="Admin" className="bg-[#1a1a1a] text-white">Admin</option>
                  <option value="Organizador" className="bg-[#1a1a1a] text-white">Organizador</option>
                  <option value="Utilizador" className="bg-[#1a1a1a] text-white">Utilizador</option>
                </select>
                <ChevronDown className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-20" />
              </div>
            </div>

            <div className="pt-8 flex items-center gap-4">
              <button 
                onClick={() => onSave({ ...user, ...formData })}
                className="px-10 py-4 bg-yellow-400 hover:bg-white text-black rounded-xl text-sm font-bold tracking-widest uppercase shadow-[0_0_15px_rgba(250,204,21,0.3)] hover:shadow-[0_0_25px_rgba(255,255,255,0.5)] hover:scale-105 transition-all flex items-center gap-2"
              >
                <Check className="w-5 h-5 mb-0.5" /> Guardar
              </button>
              <button 
                onClick={onCancel}
                className="px-10 py-4 bg-transparent border border-white/20 hover:bg-white/5 hover:border-white text-white rounded-xl text-sm font-bold tracking-widest uppercase transition-all"
              >
                Cancelar
              </button>
            </div>
          </div>

        </div>
      </div>
    </motion.div>
  );
}
