using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2ModInspector.Localization;

/// <summary>
/// Self-contained localization, following the STS2 sister-mod convention: resolve the current
/// language from <see cref="LocManager"/> (3-letter codes eng/kor/jpn/…), look the key up in that
/// language's table, then fall back to English, then to the raw key. Supports {0}-style formatting.
/// The 14 game languages: eng zhs deu esp fra ita jpn kor pol ptb rus spa tha tur.
/// </summary>
public static class Strings
{
    public static string Get(string key, params object[] args)
    {
        var raw = Lookup(key);
        if (args == null || args.Length == 0) return raw;
        try { return string.Format(raw, args); }
        catch { return raw; }
    }

    private static string Lookup(string key)
    {
        var lang = CurrentLanguage();
        if (Tables.TryGetValue(lang, out var table) && table.TryGetValue(key, out var v)) return v;
        if (Tables.TryGetValue("ENG", out var eng) && eng.TryGetValue(key, out var ev)) return ev;
        return key;
    }

    private static string CurrentLanguage()
    {
        try { return (LocManager.Instance?.Language ?? "eng").ToUpperInvariant(); }
        catch { return "ENG"; }
    }

    private static readonly Dictionary<string, Dictionary<string, string>> Tables =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["ENG"] = new()
        {
            ["badge_conflicts"] = "{0} conflicts",
            ["badge_tooltip"] = "Click for mod diagnostics details.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mods · {1} conflicts ({2} critical)",
            ["tab_critical"] = "Critical",
            ["tab_warning"] = "Warning",
            ["sev_high"] = "CRITICAL",
            ["sev_soft"] = "warning",
            ["kind_transpiler"] = "stacked transpilers",
            ["kind_prefix"] = "stacked prefixes (may block the original)",
            ["kind_overlap"] = "shared patch",
            ["conflicts_none_high"] = "No critical conflicts — no method is rewritten or blocked by two mods.",
            ["conflicts_none_soft"] = "No soft conflicts.",
            ["note_more"] = "… and {0} more",
            ["btn_save"] = "Save report",
            ["btn_close"] = "Close",
            ["save_ok"] = "Saved: {0}",
            ["save_fail"] = "Save failed (see log)",
        },
        ["KOR"] = new()
        {
            ["badge_conflicts"] = "충돌 {0}",
            ["badge_tooltip"] = "클릭하면 모드 진단 상세를 봅니다.",
            ["title"] = "🩺 모드 인스펙터",
            ["summary"] = "모드 {0}개 · 충돌 {1}개(심각 {2})",
            ["tab_critical"] = "심각",
            ["tab_warning"] = "주의",
            ["sev_high"] = "심각",
            ["sev_soft"] = "주의",
            ["kind_transpiler"] = "IL 재작성 중복",
            ["kind_prefix"] = "prefix 중복(원본 차단 위험)",
            ["kind_overlap"] = "동시 패치",
            ["conflicts_none_high"] = "심각 충돌 없음 — 어떤 함수도 두 모드가 재작성/차단하지 않습니다.",
            ["conflicts_none_soft"] = "주의 수준 충돌 없음.",
            ["note_more"] = "… 그 외 {0}개 더",
            ["btn_save"] = "리포트 저장",
            ["btn_close"] = "닫기",
            ["save_ok"] = "저장됨: {0}",
            ["save_fail"] = "저장 실패 (로그 확인)",
        },
        ["JPN"] = new()
        {
            ["badge_conflicts"] = "競合 {0}",
            ["badge_tooltip"] = "クリックでMod診断の詳細を表示します。",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "Mod {0}個 · 競合 {1}件（重大 {2}）",
            ["tab_critical"] = "重大",
            ["tab_warning"] = "注意",
            ["sev_high"] = "重大",
            ["sev_soft"] = "注意",
            ["kind_transpiler"] = "トランスパイラ重複",
            ["kind_prefix"] = "Prefix重複（元処理を妨げる恐れ）",
            ["kind_overlap"] = "同時パッチ",
            ["conflicts_none_high"] = "重大な競合なし — 2つのModに書き換え／妨害されるメソッドはありません。",
            ["conflicts_none_soft"] = "注意レベルの競合なし。",
            ["note_more"] = "… ほか {0} 件",
            ["btn_save"] = "レポート保存",
            ["btn_close"] = "閉じる",
            ["save_ok"] = "保存しました: {0}",
            ["save_fail"] = "保存に失敗（ログ参照）",
        },
        ["ZHS"] = new()
        {
            ["badge_conflicts"] = "冲突 {0}",
            ["badge_tooltip"] = "点击查看模组诊断详情。",
            ["title"] = "🩺 模组检查器",
            ["summary"] = "模组 {0} 个 · 冲突 {1} 个（严重 {2}）",
            ["tab_critical"] = "严重",
            ["tab_warning"] = "注意",
            ["sev_high"] = "严重",
            ["sev_soft"] = "注意",
            ["kind_transpiler"] = "转译器叠加",
            ["kind_prefix"] = "前缀叠加（可能拦截原方法）",
            ["kind_overlap"] = "共同修补",
            ["conflicts_none_high"] = "无严重冲突 — 没有方法被两个模组改写或拦截。",
            ["conflicts_none_soft"] = "无注意级冲突。",
            ["note_more"] = "… 还有 {0} 个",
            ["btn_save"] = "保存报告",
            ["btn_close"] = "关闭",
            ["save_ok"] = "已保存：{0}",
            ["save_fail"] = "保存失败（见日志）",
        },
        ["DEU"] = new()
        {
            ["badge_conflicts"] = "{0} Konflikte",
            ["badge_tooltip"] = "Klicken für Mod-Diagnosedetails.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} Mods · {1} Konflikte ({2} kritisch)",
            ["tab_critical"] = "Kritisch",
            ["tab_warning"] = "Warnung",
            ["sev_high"] = "KRITISCH",
            ["sev_soft"] = "Warnung",
            ["kind_transpiler"] = "mehrere Transpiler",
            ["kind_prefix"] = "mehrere Prefixe (kann Original blockieren)",
            ["kind_overlap"] = "geteilter Patch",
            ["conflicts_none_high"] = "Keine kritischen Konflikte — keine Methode wird von zwei Mods umgeschrieben oder blockiert.",
            ["conflicts_none_soft"] = "Keine leichten Konflikte.",
            ["note_more"] = "… und {0} weitere",
            ["btn_save"] = "Bericht speichern",
            ["btn_close"] = "Schließen",
            ["save_ok"] = "Gespeichert: {0}",
            ["save_fail"] = "Speichern fehlgeschlagen (siehe Log)",
        },
        ["FRA"] = new()
        {
            ["badge_conflicts"] = "{0} conflits",
            ["badge_tooltip"] = "Cliquez pour les détails du diagnostic des mods.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mods · {1} conflits ({2} critiques)",
            ["tab_critical"] = "Critique",
            ["tab_warning"] = "Avertissement",
            ["sev_high"] = "CRITIQUE",
            ["sev_soft"] = "avertissement",
            ["kind_transpiler"] = "transpileurs cumulés",
            ["kind_prefix"] = "prefixes cumulés (peut bloquer l'original)",
            ["kind_overlap"] = "patch partagé",
            ["conflicts_none_high"] = "Aucun conflit critique — aucune méthode n'est réécrite ou bloquée par deux mods.",
            ["conflicts_none_soft"] = "Aucun conflit léger.",
            ["note_more"] = "… et {0} de plus",
            ["btn_save"] = "Enregistrer le rapport",
            ["btn_close"] = "Fermer",
            ["save_ok"] = "Enregistré : {0}",
            ["save_fail"] = "Échec de l'enregistrement (voir le journal)",
        },
        ["ITA"] = new()
        {
            ["badge_conflicts"] = "{0} conflitti",
            ["badge_tooltip"] = "Clicca per i dettagli della diagnostica dei mod.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mod · {1} conflitti ({2} critici)",
            ["tab_critical"] = "Critico",
            ["tab_warning"] = "Avviso",
            ["sev_high"] = "CRITICO",
            ["sev_soft"] = "avviso",
            ["kind_transpiler"] = "transpiler sovrapposti",
            ["kind_prefix"] = "prefix sovrapposti (può bloccare l'originale)",
            ["kind_overlap"] = "patch condivisa",
            ["conflicts_none_high"] = "Nessun conflitto critico — nessun metodo è riscritto o bloccato da due mod.",
            ["conflicts_none_soft"] = "Nessun conflitto lieve.",
            ["note_more"] = "… e altri {0}",
            ["btn_save"] = "Salva report",
            ["btn_close"] = "Chiudi",
            ["save_ok"] = "Salvato: {0}",
            ["save_fail"] = "Salvataggio non riuscito (vedi log)",
        },
        ["POL"] = new()
        {
            ["badge_conflicts"] = "Konflikty: {0}",
            ["badge_tooltip"] = "Kliknij, aby zobaczyć szczegóły diagnostyki modów.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} modów · {1} konfliktów ({2} krytycznych)",
            ["tab_critical"] = "Krytyczne",
            ["tab_warning"] = "Ostrzeżenie",
            ["sev_high"] = "KRYTYCZNE",
            ["sev_soft"] = "ostrzeżenie",
            ["kind_transpiler"] = "nałożone transpilery",
            ["kind_prefix"] = "nałożone prefixy (może blokować oryginał)",
            ["kind_overlap"] = "wspólny patch",
            ["conflicts_none_high"] = "Brak krytycznych konfliktów — żadna metoda nie jest przepisywana ani blokowana przez dwa mody.",
            ["conflicts_none_soft"] = "Brak lekkich konfliktów.",
            ["note_more"] = "… i {0} więcej",
            ["btn_save"] = "Zapisz raport",
            ["btn_close"] = "Zamknij",
            ["save_ok"] = "Zapisano: {0}",
            ["save_fail"] = "Zapis nieudany (zobacz log)",
        },
        ["PTB"] = new()
        {
            ["badge_conflicts"] = "{0} conflitos",
            ["badge_tooltip"] = "Clique para ver os detalhes do diagnóstico de mods.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mods · {1} conflitos ({2} críticos)",
            ["tab_critical"] = "Crítico",
            ["tab_warning"] = "Aviso",
            ["sev_high"] = "CRÍTICO",
            ["sev_soft"] = "aviso",
            ["kind_transpiler"] = "transpilers empilhados",
            ["kind_prefix"] = "prefixes empilhados (pode bloquear o original)",
            ["kind_overlap"] = "patch compartilhado",
            ["conflicts_none_high"] = "Nenhum conflito crítico — nenhum método é reescrito ou bloqueado por dois mods.",
            ["conflicts_none_soft"] = "Nenhum conflito leve.",
            ["note_more"] = "… e mais {0}",
            ["btn_save"] = "Salvar relatório",
            ["btn_close"] = "Fechar",
            ["save_ok"] = "Salvo: {0}",
            ["save_fail"] = "Falha ao salvar (veja o log)",
        },
        ["RUS"] = new()
        {
            ["badge_conflicts"] = "Конфликтов: {0}",
            ["badge_tooltip"] = "Нажмите для подробностей диагностики модов.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "Модов: {0} · конфликтов: {1} (критич. {2})",
            ["tab_critical"] = "Критич.",
            ["tab_warning"] = "Внимание",
            ["sev_high"] = "КРИТИЧНО",
            ["sev_soft"] = "внимание",
            ["kind_transpiler"] = "несколько транспайлеров",
            ["kind_prefix"] = "несколько prefix (может блокировать оригинал)",
            ["kind_overlap"] = "общий патч",
            ["conflicts_none_high"] = "Критических конфликтов нет — ни один метод не переписывается и не блокируется двумя модами.",
            ["conflicts_none_soft"] = "Лёгких конфликтов нет.",
            ["note_more"] = "… и ещё {0}",
            ["btn_save"] = "Сохранить отчёт",
            ["btn_close"] = "Закрыть",
            ["save_ok"] = "Сохранено: {0}",
            ["save_fail"] = "Не удалось сохранить (см. лог)",
        },
        ["SPA"] = new()
        {
            ["badge_conflicts"] = "{0} conflictos",
            ["badge_tooltip"] = "Haz clic para ver los detalles del diagnóstico de mods.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mods · {1} conflictos ({2} críticos)",
            ["tab_critical"] = "Crítico",
            ["tab_warning"] = "Aviso",
            ["sev_high"] = "CRÍTICO",
            ["sev_soft"] = "aviso",
            ["kind_transpiler"] = "transpiladores apilados",
            ["kind_prefix"] = "prefixes apilados (puede bloquear el original)",
            ["kind_overlap"] = "parche compartido",
            ["conflicts_none_high"] = "Sin conflictos críticos — ningún método es reescrito o bloqueado por dos mods.",
            ["conflicts_none_soft"] = "Sin conflictos leves.",
            ["note_more"] = "… y {0} más",
            ["btn_save"] = "Guardar informe",
            ["btn_close"] = "Cerrar",
            ["save_ok"] = "Guardado: {0}",
            ["save_fail"] = "Error al guardar (ver registro)",
        },
        ["ESP"] = new()
        {
            ["badge_conflicts"] = "{0} conflictos",
            ["badge_tooltip"] = "Haz clic para ver los detalles del diagnóstico de mods.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mods · {1} conflictos ({2} críticos)",
            ["tab_critical"] = "Crítico",
            ["tab_warning"] = "Aviso",
            ["sev_high"] = "CRÍTICO",
            ["sev_soft"] = "aviso",
            ["kind_transpiler"] = "transpiladores apilados",
            ["kind_prefix"] = "prefixes apilados (puede bloquear el original)",
            ["kind_overlap"] = "parche compartido",
            ["conflicts_none_high"] = "Sin conflictos críticos — ningún método es reescrito o bloqueado por dos mods.",
            ["conflicts_none_soft"] = "Sin conflictos leves.",
            ["note_more"] = "… y {0} más",
            ["btn_save"] = "Guardar informe",
            ["btn_close"] = "Cerrar",
            ["save_ok"] = "Guardado: {0}",
            ["save_fail"] = "Error al guardar (ver registro)",
        },
        ["THA"] = new()
        {
            ["badge_conflicts"] = "ขัดแย้ง {0}",
            ["badge_tooltip"] = "คลิกเพื่อดูรายละเอียดการวินิจฉัยม็อด",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "ม็อด {0} · ขัดแย้ง {1} (ร้ายแรง {2})",
            ["tab_critical"] = "ร้ายแรง",
            ["tab_warning"] = "คำเตือน",
            ["sev_high"] = "ร้ายแรง",
            ["sev_soft"] = "คำเตือน",
            ["kind_transpiler"] = "ทรานสไพเลอร์ซ้อนกัน",
            ["kind_prefix"] = "prefix ซ้อนกัน (อาจบล็อกต้นฉบับ)",
            ["kind_overlap"] = "แพตช์ร่วมกัน",
            ["conflicts_none_high"] = "ไม่มีความขัดแย้งร้ายแรง — ไม่มีเมธอดใดถูกเขียนทับหรือบล็อกโดยสองม็อด",
            ["conflicts_none_soft"] = "ไม่มีความขัดแย้งระดับเบา",
            ["note_more"] = "… และอีก {0}",
            ["btn_save"] = "บันทึกรายงาน",
            ["btn_close"] = "ปิด",
            ["save_ok"] = "บันทึกแล้ว: {0}",
            ["save_fail"] = "บันทึกไม่สำเร็จ (ดูบันทึก)",
        },
        ["TUR"] = new()
        {
            ["badge_conflicts"] = "{0} çakışma",
            ["badge_tooltip"] = "Mod tanılama ayrıntıları için tıklayın.",
            ["title"] = "🩺 Mod Inspector",
            ["summary"] = "{0} mod · {1} çakışma ({2} kritik)",
            ["tab_critical"] = "Kritik",
            ["tab_warning"] = "Uyarı",
            ["sev_high"] = "KRİTİK",
            ["sev_soft"] = "uyarı",
            ["kind_transpiler"] = "üst üste transpiler",
            ["kind_prefix"] = "üst üste prefix (orijinali engelleyebilir)",
            ["kind_overlap"] = "ortak yama",
            ["conflicts_none_high"] = "Kritik çakışma yok — hiçbir metot iki mod tarafından yeniden yazılmıyor veya engellenmiyor.",
            ["conflicts_none_soft"] = "Hafif çakışma yok.",
            ["note_more"] = "… ve {0} tane daha",
            ["btn_save"] = "Raporu kaydet",
            ["btn_close"] = "Kapat",
            ["save_ok"] = "Kaydedildi: {0}",
            ["save_fail"] = "Kaydetme başarısız (günlüğe bakın)",
        },
    };
}
