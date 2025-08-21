import { Link, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';

export default function LandingPage() {
  const { state } = useAuth();
  const navigate = useNavigate();
  useEffect(() => {
    if (state.user) {
      navigate('/app');
    }
  }, [state.user, navigate]);

  return (
    <div className="min-h-screen flex flex-col">
      <header className="px-8 py-4 flex justify-between items-center border-b bg-white/70 backdrop-blur">
        <h1 className="text-xl font-bold">Yargısal Zeka</h1>
        <nav className="space-x-4 text-sm">
          <Link to="#features" className="hover:underline">Özellikler</Link>
          <Link to="#plans" className="hover:underline">Paketler</Link>
          <Link to="#faq" className="hover:underline">SSS</Link>
          <Link to="/login" className="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700">Giriş</Link>
          <Link to="/register" className="px-4 py-2 rounded border border-blue-600 text-blue-600 hover:bg-blue-50">Kayıt Ol</Link>
        </nav>
      </header>

      <main className="flex-1">
        <section className="px-8 py-24 bg-gradient-to-b from-blue-50 to-white text-center">
          <h2 className="text-4xl font-bold mb-6 leading-tight">Yargı Kararlarından Yapay Zeka Destekli İçgörüler</h2>
          <p className="max-w-2xl mx-auto text-gray-600 mb-8">Arama, analiz, dilekçe taslağı oluşturma ve abonelik bazlı kullanım kotası yönetimi ile hukuk teknolojisinde yeni nesil deneyim.</p>
          <div className="space-x-4">
            <Link to="/register" className="px-6 py-3 rounded bg-blue-600 text-white font-medium hover:bg-blue-700">Ücretsiz Başla</Link>
            <Link to="#plans" className="px-6 py-3 rounded border font-medium hover:bg-gray-50">Paketleri Gör</Link>
          </div>
        </section>

        <section id="features" className="px-8 py-16 grid md:grid-cols-3 gap-8 max-w-6xl mx-auto">
          {FEATURES.map(f => (
            <div key={f.title} className="p-6 border rounded-lg bg-white shadow-sm hover:shadow-md transition">
              <h3 className="font-semibold mb-2">{f.title}</h3>
              <p className="text-sm text-gray-600">{f.desc}</p>
            </div>
          ))}
        </section>

        <section id="plans" className="px-8 py-16 bg-gray-50">
          <h3 className="text-2xl font-bold text-center mb-10">Abonelik Paketleri</h3>
          <div className="grid md:grid-cols-3 gap-8 max-w-6xl mx-auto">
            {PLANS.map(p => (
              <div key={p.name} className="p-6 border rounded-lg bg-white flex flex-col">
                <h4 className="font-semibold text-lg mb-2">{p.name}</h4>
                <p className="text-gray-600 text-sm mb-4 flex-1">{p.desc}</p>
                <ul className="text-sm mb-4 list-disc pl-4 space-y-1 text-gray-700">
                  {p.features.map(ft => <li key={ft}>{ft}</li>)}
                </ul>
                <div className="text-2xl font-bold mb-4">{p.price}</div>
                <Link to="/register" className="mt-auto px-4 py-2 rounded bg-blue-600 text-white text-center hover:bg-blue-700">Başla</Link>
              </div>
            ))}
          </div>
        </section>

        <section id="faq" className="px-8 py-16 max-w-4xl mx-auto">
          <h3 className="text-2xl font-bold mb-6">Sık Sorulan Sorular</h3>
          <div className="space-y-4">
            {FAQ.map(item => (
              <details key={item.q} className="border rounded-lg p-4 bg-white">
                <summary className="cursor-pointer font-medium">{item.q}</summary>
                <p className="pt-2 text-sm text-gray-600">{item.a}</p>
              </details>
            ))}
          </div>
        </section>
      </main>

      <footer className="px-8 py-6 border-t text-sm text-gray-500 bg-white">
        © {new Date().getFullYear()} Yargısal Zeka. Tüm hakları saklıdır.
      </footer>
    </div>
  );
}

const FEATURES = [
  { title: 'Akıllı Arama', desc: 'Yüksek hacimli yargı kararları üzerinde hızlı ve isabetli arama.' },
  { title: 'Karar Analizi', desc: 'Yapay zeka ile özet, anahtar kavram çıkarımı ve içgörü üretimi.' },
  { title: 'Dilekçe Taslağı', desc: 'Girdiğiniz bilgilerden otomatik dilekçe taslakları oluşturun.' },
  { title: 'Kullanım Kotası', desc: 'Abonelik planlarına göre özelleştirilmiş özellik limitleri.' },
  { title: 'Kayıt & Geçmiş', desc: 'Arama geçmişi ve kaydedilmiş kararları organize edin.' },
  { title: 'Güvenli Erişim', desc: 'JWT tabanlı yetkilendirme ve güvenli veri akışı.' }
];

const PLANS = [
  { name: 'Başlangıç', price: '₺0', desc: 'Deneme ve temel ihtiyaçlar.', features: ['Sınırlı arama', 'Örnek analiz', '5 dilekçe/ay'] },
  { name: 'Profesyonel', price: '₺499', desc: 'Yoğun bireysel kullanım.', features: ['Geniş arama kotası', 'Tam analiz', 'Sınırsız kaydetme', '20 dilekçe/ay'] },
  { name: 'Kurumsal', price: 'İletişim', desc: 'Ekipler ve ölçeklenebilir ihtiyaçlar.', features: ['Özel entegrasyon', 'Yüksek hız', 'Öncelikli destek', 'Sınırsız dilekçe'] }
];

const FAQ = [
  { q: 'Veriler nerede saklanıyor?', a: 'Veriler güvenli bir altyapıda saklanır ve yalnızca yetkili isteklerle erişilir.' },
  { q: 'Ücretsiz plan ne sunuyor?', a: 'Temel arama ve sınırlı sayıda analiz fonksiyonu içerir.' },
  { q: 'Planımı yükseltebilir miyim?', a: 'Evet, yükseltme sonrasında kalan kullanım hakları yeniden hesaplanır.' }
];
