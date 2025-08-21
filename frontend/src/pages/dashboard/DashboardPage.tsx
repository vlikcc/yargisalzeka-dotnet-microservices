import { useSubscription } from '../../contexts/SubscriptionContext';
import { useAuth } from '../../contexts/AuthContext';
import { StatCard, UsageChart, SearchHistoryList } from '../../components/dashboard';
import { useMemo } from 'react';

export default function DashboardPage() {
  const { usage, plan, remaining, loading: subscriptionLoading } = useSubscription();
  const { state: authState } = useAuth();

  const totals = useMemo(() => {
    if (!usage) return null;
    const total = usage.searches + usage.caseAnalyses + usage.petitions + usage.keywordExtractions;
    return { total };
  }, [usage]);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
  <h2 className="text-2xl font-semibold">Hoş geldin{authState.user ? `, ${authState.user.firstName || authState.user.email}` : ''}</h2>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <StatCard
          title="Toplam İşlem"
          value={totals?.total ?? '-'}
          description="Tüm kullanım türlerinin toplamı"
          loading={subscriptionLoading}
        />
        <StatCard
          title="Aramalar"
          value={usage?.searches}
          description="Gerçekleştirilen arama sayısı"
          loading={subscriptionLoading}
        />
        <StatCard
          title="Analiz"
          value={usage?.caseAnalyses}
          description="Karar analizleri"
          loading={subscriptionLoading}
        />
        <StatCard
          title="Dilekçe"
          value={usage?.petitions}
          description="Oluşturulan dilekçeler"
          loading={subscriptionLoading}
        />
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div className="md:col-span-2"><UsageChart usage={usage} loading={subscriptionLoading} /></div>
        <SearchHistoryList />
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <StatCard
          title="Anahtar Kelime Çıkarma"
          value={usage?.keywordExtractions}
          description="Anahtar kelime çıkarma işlemleri"
          loading={subscriptionLoading}
        />
        <StatCard
          title="Plan"
          value={plan?.name || '-'}
          description={plan ? `Ücret: ${plan.price}₺` : 'Plan bilgisi yok'}
          loading={subscriptionLoading}
        />
        <StatCard
          title="Kalan Krediler"
          value={remaining ? '' : '-'}
          description={remaining ? `Arama: ${remaining.search === -1 ? '∞' : remaining.search} | Analiz: ${remaining.caseAnalysis === -1 ? '∞' : remaining.caseAnalysis} | Dilekçe: ${remaining.petition === -1 ? '∞' : remaining.petition}` : 'Bilgi yok'}
          loading={subscriptionLoading}
        />
      </div>
    </div>
  );
}
