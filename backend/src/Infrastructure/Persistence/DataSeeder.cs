using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedCellDefinitionsAsync(db);

        if (db.Users.Any())
        {
            // ユーザーは既にシード済み。差分のみ補完して終了
            await SeedNineBoxAssessmentsAsync(db);
            await SeedTalentManagerCommentsAsync(db);
            await SeedWcmEntriesAsync(db);
            await SeedCareerValuesAsync(db);
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentYear = DateTime.UtcNow.Year;
        var prevYear = currentYear - 1;

        // ===== ユーザー =====
        var admin    = User.Create("dev-admin",    "管理者 システム", "admin@example.com",    UserRole.Admin);
        var exec     = User.Create("dev-exec",     "役員 太郎",       "exec@example.com",     UserRole.Executive);
        var senior   = User.Create("dev-senior",   "部長 一郎",       "senior@example.com",   UserRole.Senior);
        var manager1 = User.Create("dev-mgr1",     "山田 一郎",       "yamada@example.com",   UserRole.Manager);
        var manager2 = User.Create("dev-mgr2",     "鈴木 二郎",       "suzuki@example.com",   UserRole.Manager);
        var talent1  = User.Create("dev-talent1",  "田中 太郎",       "tanaka@example.com",   UserRole.Talent, manager1.Id);
        var talent2  = User.Create("dev-talent2",  "佐藤 花子",       "sato@example.com",     UserRole.Talent, manager1.Id);
        var talent3  = User.Create("dev-talent3",  "伊藤 健太",       "ito@example.com",      UserRole.Talent, manager2.Id);
        var talent4  = User.Create("dev-talent4",  "渡辺 美咲",       "watanabe@example.com", UserRole.Talent, manager2.Id);

        db.Users.AddRange(admin, exec, senior, manager1, manager2, talent1, talent2, talent3, talent4);

        // ===== スキルマスター =====
        var catFE   = SkillCategory.Create("フロントエンド", 1);
        var catBE   = SkillCategory.Create("バックエンド",   2);
        var catInfra = SkillCategory.Create("インフラ・クラウド", 3);

        db.SkillCategories.AddRange(catFE, catBE, catInfra);

        var skillTs    = SkillItem.Create(catFE.Id, "TypeScript",  1);
        var skillVue   = SkillItem.Create(catFE.Id, "Vue.js",      2);
        var skillNuxt  = SkillItem.Create(catFE.Id, "Nuxt.js",     3);
        var skillCs    = SkillItem.Create(catBE.Id, "C#",          1);
        var skillAspnet = SkillItem.Create(catBE.Id, "ASP.NET Core", 2);
        var skillEfcore = SkillItem.Create(catBE.Id, "Entity Framework Core", 3);
        var skillAzure  = SkillItem.Create(catInfra.Id, "Azure",          1);
        var skillDocker = SkillItem.Create(catInfra.Id, "Docker",         2);
        var skillGha    = SkillItem.Create(catInfra.Id, "GitHub Actions", 3);

        db.SkillItems.AddRange(skillTs, skillVue, skillNuxt, skillCs, skillAspnet, skillEfcore, skillAzure, skillDocker, skillGha);

        // ===== 評価期間 =====
        // 前年度（全フェーズ終了済み）
        var prevPeriod = EvaluationPeriod.Create(
            prevYear, $"{prevYear}年度通期考課",
            new DateOnly(prevYear, 4, 1), new DateOnly(prevYear, 5, 31),
            new DateOnly(prevYear, 10, 1), new DateOnly(prevYear, 11, 30),
            new DateOnly(prevYear, 12, 31));

        // 今年度（目標設定オープン中）
        var currPeriod = EvaluationPeriod.Create(
            currentYear, $"{currentYear}年度通期考課",
            today.AddDays(-30), today.AddDays(30),
            today.AddDays(60), today.AddDays(120),
            today.AddDays(150));

        db.EvaluationPeriods.AddRange(prevPeriod, currPeriod);
        await db.SaveChangesAsync();

        // ===== ユーザースキル =====
        // talent1: TypeScript Lv4(承認済), Vue.js Lv4(承認済), Nuxt.js Lv3(承認済), C# Lv2(承認済), Azure Lv2(申請中)
        var us1_ts    = UserSkill.Create(talent1.Id, skillTs.Id,    4, "実務3年以上、型定義・ジェネリクスも扱える");
        var us1_vue   = UserSkill.Create(talent1.Id, skillVue.Id,   4, "Composition API、Pinia を使いこなせる");
        var us1_nuxt  = UserSkill.Create(talent1.Id, skillNuxt.Id,  3, "SSR・APIルート実装経験あり");
        var us1_cs    = UserSkill.Create(talent1.Id, skillCs.Id,    2, "バックエンド補助として使用");
        var us1_azure = UserSkill.Create(talent1.Id, skillAzure.Id, 2, "Azure Container Apps へのデプロイ経験あり");
        us1_ts.Approve(manager1.Id);
        us1_vue.Approve(manager1.Id);
        us1_nuxt.Approve(manager1.Id);
        us1_cs.Approve(manager1.Id);
        db.UserSkills.AddRange(us1_ts, us1_vue, us1_nuxt, us1_cs, us1_azure);

        // talent2: TypeScript Lv3(承認済), Vue.js Lv3(承認済), C# Lv4(承認済), ASP.NET Core Lv4(承認済), EF Core Lv3(承認済), Docker Lv2(差し戻し)
        var us2_ts    = UserSkill.Create(talent2.Id, skillTs.Id,    3, "実務2年、中規模プロジェクト経験あり");
        var us2_vue   = UserSkill.Create(talent2.Id, skillVue.Id,   3, "Options API から Composition API へ移行経験あり");
        var us2_cs    = UserSkill.Create(talent2.Id, skillCs.Id,    4, "設計からテストまで一人で完結できる");
        var us2_aspnet = UserSkill.Create(talent2.Id, skillAspnet.Id, 4, "REST API設計・認証実装経験あり");
        var us2_efcore = UserSkill.Create(talent2.Id, skillEfcore.Id, 3, "マイグレーション・クエリ最適化経験あり");
        var us2_docker = UserSkill.Create(talent2.Id, skillDocker.Id, 2, "マルチステージビルド実装経験あり");
        us2_ts.Approve(manager1.Id);
        us2_vue.Approve(manager1.Id);
        us2_cs.Approve(manager1.Id);
        us2_aspnet.Approve(manager1.Id);
        us2_efcore.Approve(manager1.Id);
        us2_docker.Reject("実務での継続使用実績が不明確です。具体的なプロジェクト名を追記してください。");
        db.UserSkills.AddRange(us2_ts, us2_vue, us2_cs, us2_aspnet, us2_efcore, us2_docker);

        // talent3: C# Lv3(承認済), ASP.NET Core Lv3(承認済), Azure Lv3(承認済), Docker Lv3(承認済), GitHub Actions Lv2(承認済), TypeScript Lv2(申請中)
        var us3_cs    = UserSkill.Create(talent3.Id, skillCs.Id,    3, "API開発3年経験");
        var us3_aspnet = UserSkill.Create(talent3.Id, skillAspnet.Id, 3, "Web API開発経験あり");
        var us3_azure  = UserSkill.Create(talent3.Id, skillAzure.Id, 3, "Azure SQL, Container Apps 運用経験あり");
        var us3_docker = UserSkill.Create(talent3.Id, skillDocker.Id, 3, "本番環境でのコンテナ運用経験あり");
        var us3_gha    = UserSkill.Create(talent3.Id, skillGha.Id,   2, "CI/CDパイプライン構築経験あり");
        var us3_ts     = UserSkill.Create(talent3.Id, skillTs.Id,    2, "フロント学習中");
        us3_cs.Approve(manager2.Id);
        us3_aspnet.Approve(manager2.Id);
        us3_azure.Approve(manager2.Id);
        us3_docker.Approve(manager2.Id);
        us3_gha.Approve(manager2.Id);
        db.UserSkills.AddRange(us3_cs, us3_aspnet, us3_azure, us3_docker, us3_gha, us3_ts);

        // talent4: Vue.js Lv2(承認済), TypeScript Lv2(承認済), Nuxt.js Lv1(申請中)
        var us4_vue  = UserSkill.Create(talent4.Id, skillVue.Id,  2, "入社後独学で習得");
        var us4_ts   = UserSkill.Create(talent4.Id, skillTs.Id,   2, "基本的な型定義ができる");
        var us4_nuxt = UserSkill.Create(talent4.Id, skillNuxt.Id, 1, "現在学習中");
        us4_vue.Approve(manager2.Id);
        us4_ts.Approve(manager2.Id);
        db.UserSkills.AddRange(us4_vue, us4_ts, us4_nuxt);

        // ===== 前年度 目標・自己評価・上長評価 (talent1) =====
        var g1p1_will = Goal.Create(talent1.Id, prevPeriod.Id, WillCanMust.Will,
            "フロントエンドスペシャリストとしての地位確立", "社内で最も信頼されるフロントエンドエンジニアになる", 1);
        var g1p1_can = Goal.Create(talent1.Id, prevPeriod.Id, WillCanMust.Can,
            "Nuxt.js によるSSRアプリケーション開発", "パフォーマンスを意識したSSRアプリ3本以上の開発", 2);
        var g1p1_must = Goal.Create(talent1.Id, prevPeriod.Id, WillCanMust.Must,
            "タレントマネジメントシステムのFE実装完了", "Sprint 1〜8のFEタスクを担当し全画面を完成させる", 3);
        db.Goals.AddRange(g1p1_will, g1p1_can, g1p1_must);

        var se1p1 = SelfEvaluation.Create(talent1.Id, prevPeriod.Id);
        db.SelfEvaluations.Add(se1p1);
        await db.SaveChangesAsync();

        var sei1_will = SelfEvaluationItem.Create(se1p1.Id, g1p1_will.Id, 4, "社内勉強会を3回開催、フロント技術のリードができた");
        var sei1_can  = SelfEvaluationItem.Create(se1p1.Id, g1p1_can.Id,  4, "SSRアプリを4本リリース。Core Web Vitalsの改善を達成");
        var sei1_must = SelfEvaluationItem.Create(se1p1.Id, g1p1_must.Id, 5, "全Sprint完遂。予定より1週間早く完成できた");
        db.SelfEvaluationItems.AddRange(sei1_will, sei1_can, sei1_must);
        se1p1.Submit();
        await db.SaveChangesAsync();

        var me1p1 = ManagerEvaluation.Create(se1p1.Id, manager1.Id);
        db.ManagerEvaluations.Add(me1p1);
        await db.SaveChangesAsync();
        me1p1.UpdateDraft(4, "全目標を高いレベルで達成。特にSSRアプリのパフォーマンス改善は顕著な成果。来年度はより難易度の高いアーキテクチャ設計にも挑戦してほしい。");
        me1p1.Confirm();

        // 前年度 目標・自己評価・上長評価 (talent2)
        var g2p1_will = Goal.Create(talent2.Id, prevPeriod.Id, WillCanMust.Will,
            "フルスタックエンジニアとしてのスキル強化", "FE・BEどちらも一人でリードできる力をつける", 1);
        var g2p1_can = Goal.Create(talent2.Id, prevPeriod.Id, WillCanMust.Can,
            "ASP.NET Core Web API のアーキテクチャ設計", "クリーンアーキテクチャを採用したAPIを設計・実装する", 2);
        var g2p1_must = Goal.Create(talent2.Id, prevPeriod.Id, WillCanMust.Must,
            "バックエンド全APIの実装とテスト完了", "Sprint 1〜8のBEタスクをリードする", 3);
        db.Goals.AddRange(g2p1_will, g2p1_can, g2p1_must);

        var se2p1 = SelfEvaluation.Create(talent2.Id, prevPeriod.Id);
        db.SelfEvaluations.Add(se2p1);
        await db.SaveChangesAsync();

        var sei2_will = SelfEvaluationItem.Create(se2p1.Id, g2p1_will.Id, 5, "FE・BEともに実装リードできた。来年度はインフラも学びたい");
        var sei2_can  = SelfEvaluationItem.Create(se2p1.Id, g2p1_can.Id,  5, "クリーンアーキテクチャで全API設計・実装完了");
        var sei2_must = SelfEvaluationItem.Create(se2p1.Id, g2p1_must.Id, 5, "全BEタスク完遂。テストカバレッジ90%以上を達成");
        db.SelfEvaluationItems.AddRange(sei2_will, sei2_can, sei2_must);
        se2p1.Submit();
        await db.SaveChangesAsync();

        var me2p1 = ManagerEvaluation.Create(se2p1.Id, manager1.Id);
        db.ManagerEvaluations.Add(me2p1);
        await db.SaveChangesAsync();
        me2p1.UpdateDraft(5, "今年度最も顕著な成長を見せたエンジニア。アーキテクチャ設計力が急速に向上した。次年度はチームリードを任せたい。");
        me2p1.Confirm();

        // 今年度 目標 (talent1, talent2, talent3)
        var g1c_will = Goal.Create(talent1.Id, currPeriod.Id, WillCanMust.Will,
            "テックリードへのキャリアアップ", "3年以内にテックリードとして組織の技術戦略を担う", 1);
        var g1c_can = Goal.Create(talent1.Id, currPeriod.Id, WillCanMust.Can,
            "マイクロフロントエンドアーキテクチャの習得", "大規模FEの分割統治パターンを習得・実践する", 2);
        var g1c_must = Goal.Create(talent1.Id, currPeriod.Id, WillCanMust.Must,
            "新規プロダクトのFEアーキテクチャ設計", "Q2までに新プロダクトのFE技術選定・設計を完了する", 3);
        db.Goals.AddRange(g1c_will, g1c_can, g1c_must);

        var g2c_will = Goal.Create(talent2.Id, currPeriod.Id, WillCanMust.Will,
            "バックエンドアーキテクトへの成長", "分散システム・イベント駆動設計を習得する", 1);
        var g2c_can = Goal.Create(talent2.Id, currPeriod.Id, WillCanMust.Can,
            "Azure Service Bus / Event Grid 活用", "非同期処理基盤の設計・構築を担当する", 2);
        var g2c_must = Goal.Create(talent2.Id, currPeriod.Id, WillCanMust.Must,
            "次期システムのBEアーキテクチャ設計と実装", "Q3までに主要APIの設計・実装を担当する", 3);
        db.Goals.AddRange(g2c_will, g2c_can, g2c_must);

        var g3c_will = Goal.Create(talent3.Id, currPeriod.Id, WillCanMust.Will,
            "クラウドインフラエンジニアとしての専門性向上", "Azure認定資格（Solutions Architect）の取得", 1);
        var g3c_must = Goal.Create(talent3.Id, currPeriod.Id, WillCanMust.Must,
            "CI/CDパイプライン全面刷新", "GitHub Actions を使った自動化率80%以上を達成する", 2);
        db.Goals.AddRange(g3c_will, g3c_must);

        // ===== キャリアプラン =====
        // 前年度キャリアプラン
        var cp1_prev = CareerPlan.Create(talent1.Id,
            "5年後にテックリードとしてエンジニア組織の技術戦略を立案・実行するポジションに就く",
            "フロントエンド領域のスペシャリストとして認められ、社内外での登壇実績を作る",
            "Nuxt.js/Vue.js でのSSR開発スキルを磨き、パフォーマンス改善の実績を積む");
        typeof(CareerPlan).GetProperty("PeriodYear")!.SetValue(cp1_prev, prevYear);
        db.CareerPlans.Add(cp1_prev);

        var cp2_prev = CareerPlan.Create(talent2.Id,
            "フルスタックエンジニアとして社内最高技術責任者（CTO）を目指す",
            "バックエンドアーキテクトとして認められ、システム設計を一人でリードできるようになる",
            "ASP.NET Core での大規模API設計と、テスト駆動開発の実践");
        typeof(CareerPlan).GetProperty("PeriodYear")!.SetValue(cp2_prev, prevYear);
        db.CareerPlans.Add(cp2_prev);

        // 今年度キャリアプラン
        var cp1_curr = CareerPlan.Create(talent1.Id,
            "3年以内にテックリードとして組織の技術戦略を担い、エンジニア採用・育成にも関与する",
            "マイクロフロントエンドや状態管理設計など上流工程を担当できるスキルを身につける",
            "新規プロダクトのFEアーキテクチャ選定・設計を担当し、チームメンバーへの技術共有を行う");
        db.CareerPlans.Add(cp1_curr);

        var cp2_curr = CareerPlan.Create(talent2.Id,
            "分散システム・イベント駆動アーキテクチャのスペシャリストとして社内外での認知を高める",
            "Azure Service BusやEvent Gridを活用した非同期処理基盤の設計・構築をリードする",
            "次期システムのBEアーキテクチャ設計を一人でリードし、実装完了まで担当する");
        db.CareerPlans.Add(cp2_curr);

        var cp3_curr = CareerPlan.Create(talent3.Id,
            "Azure認定資格（Solutions Architect Expert）を取得し、クラウドインフラの設計をリードする",
            "Azure環境のコスト最適化とセキュリティ強化を担当する",
            "CI/CDパイプライン全面刷新でデプロイ自動化率80%以上を達成する");
        db.CareerPlans.Add(cp3_curr);

        var cp4_curr = CareerPlan.Create(talent4.Id,
            "フロントエンドエンジニアとして独力でUI設計・実装ができるレベルに成長する",
            "Vue.js / Nuxt.js の実務経験を積み、コードレビューができるレベルを目指す",
            "既存プロダクトのUI改善タスクを自走で完了する");
        db.CareerPlans.Add(cp4_curr);

        await db.SaveChangesAsync();

        // 上長からのキャリアプランコメント
        var cpc1 = CareerPlanComment.Create(cp1_curr.Id, manager1.Id,
            "テックリードへの意欲と実力が伴ってきた。今期はアーキテクチャ設計の機会を積極的に作りますので、ぜひ挑戦してください。");
        var cpc2 = CareerPlanComment.Create(cp2_curr.Id, manager1.Id,
            "昨年度の成長は目覚ましかった。今年度はさらに上流の設計から携わってもらう予定です。積極的に意見を出していきましょう。");
        var cpc3 = CareerPlanComment.Create(cp3_curr.Id, manager2.Id,
            "Azure認定資格の勉強と実務経験を並行して積んでください。勉強会参加費・受験費用はサポートします。");
        db.CareerPlanComments.AddRange(cpc1, cpc2, cpc3);

        // ===== 重点管理人物 =====
        // talent1: 現在も重点管理中（今年度も継続）
        var kp1 = KeyPerson.Create(talent1.Id, senior.Id, "高いフロントエンドスキルと成長意欲を持つ次世代テックリード候補。重点的な育成支援が必要。");
        // talent2: 現在も重点管理中
        var kp2 = KeyPerson.Create(talent2.Id, senior.Id, "バックエンド設計力が突出しており、将来の技術リードとして期待。");
        // talent3: 前年度に指定→解除済み（昨年度で目標達成のため）
        var kp3_old = KeyPerson.Create(talent3.Id, senior.Id, "インフラ領域でのキャッチアップを支援。Azure資格取得を目標に設定。");
        db.KeyPersons.AddRange(kp1, kp2, kp3_old);
        await db.SaveChangesAsync();
        kp3_old.Deactivate(); // 前年度終了時に解除

        // ===== 育成方針 =====
        // talent1 の育成方針 (v1: 前年度, v2: 今年度)
        var dp1_v1 = DevelopmentPlan.Create(kp1.Id, manager1.Id,
            "Nuxt.js のSSR知識はあるが、チーム全体への技術展開が不足している",
            "社内勉強会を四半期に1回以上開催し、フロントエンド技術のナレッジシェアを推進する",
            "月1回の1on1でアウトプット状況を確認。外部カンファレンス参加費を補助する",
            new DateOnly(prevYear, 12, 31), 1);
        var dp1_v2 = DevelopmentPlan.Create(kp1.Id, manager1.Id,
            "テックリードとしての上流設計経験が不足している",
            "新規プロダクトのFEアーキテクチャ設計を一人でリードし、Q2までに設計書を完成させる",
            "週次でのアーキテクチャレビューミーティングを設定。設計書レビューは部長も参加する",
            new DateOnly(currentYear, 6, 30), 2);
        db.DevelopmentPlans.AddRange(dp1_v1, dp1_v2);

        // talent2 の育成方針 (v1: 前年度, v2: 今年度)
        var dp2_v1 = DevelopmentPlan.Create(kp2.Id, manager1.Id,
            "個人のコーディングスキルは高いが、設計ドキュメント作成・コミュニケーションが課題",
            "主要API設計時に設計書を必ず作成し、チームレビューを受ける習慣を身につける",
            "設計書テンプレートを提供し、レビュー機会を月2回以上設ける",
            new DateOnly(prevYear, 12, 31), 1);
        var dp2_v2 = DevelopmentPlan.Create(kp2.Id, manager1.Id,
            "分散システムの実務経験が不足。理論理解はあるが実装経験が必要",
            "Azure Service Busを使った非同期処理を実際のプロジェクトで実装し、本番稼働まで担当する",
            "Q2の新規機能開発でメイン担当に任命。週次でのアーキテクチャレビューで支援する",
            new DateOnly(currentYear, 9, 30), 2);
        db.DevelopmentPlans.AddRange(dp2_v1, dp2_v2);

        // ===== 委員会セッション（前年度3回 + 今年度1回） =====
        var session1 = CommitteeSession.Create(
            new DateOnly(prevYear, 5, 15),
            $"【{prevYear}年度 第1回 人材開発委員会】\n\n" +
            "■ 出席者: 部長一郎、山田一郎（Manager）、鈴木二郎（Manager）\n\n" +
            "■ 重点管理人物の選定\n" +
            $"・田中太郎（talent1）を重点管理人物に指定。理由：フロントエンドスキルが高く、次世代テックリード候補として育成が必要。\n" +
            "・佐藤花子（talent2）を重点管理人物に指定。理由：バックエンド設計力が突出しており、早期育成が望ましい。\n" +
            "・伊藤健太（talent3）を重点管理人物に指定。理由：インフラ領域でのキャッチアップを重点支援する。\n\n" +
            "■ 育成方針の検討\n" +
            "・各マネージャーが担当タレントの課題と目標を設定することを確認。\n" +
            "・Q3の次回セッションで中間振り返りを実施する予定。",
            senior.Id);
        session1.Participants.Add(CommitteeSessionParticipant.Create(session1.Id, senior.Id));
        session1.Participants.Add(CommitteeSessionParticipant.Create(session1.Id, manager1.Id));
        session1.Participants.Add(CommitteeSessionParticipant.Create(session1.Id, manager2.Id));
        session1.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session1.Id, kp1.Id));
        session1.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session1.Id, kp2.Id));
        session1.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session1.Id, kp3_old.Id));

        var session2 = CommitteeSession.Create(
            new DateOnly(prevYear, 9, 10),
            $"【{prevYear}年度 第2回 人材開発委員会（中間振り返り）】\n\n" +
            "■ 出席者: 部長一郎、山田一郎（Manager）、鈴木二郎（Manager）\n\n" +
            "■ 重点管理人物の進捗確認\n" +
            "・田中太郎：勉強会2回開催済み。SSRアプリ開発も順調。育成方針どおり進捗良好。\n" +
            "・佐藤花子：設計書作成の習慣が定着。APIレビューでも積極的に意見を出している。\n" +
            "・伊藤健太：Azure学習は順調。Docker/GitHubActionsのスキルアップを確認。\n\n" +
            "■ 育成方針の更新\n" +
            "・田中太郎の来年度育成方針を前倒しで検討開始する（アーキテクチャ設計経験の付与）。\n" +
            "・佐藤花子の来年度は分散システム実装経験を積ませる方向で調整。\n\n" +
            "■ 次回予定\n" +
            "・{prevYear}年12月に年度末振り返りセッションを実施する。",
            senior.Id);
        session2.Participants.Add(CommitteeSessionParticipant.Create(session2.Id, senior.Id));
        session2.Participants.Add(CommitteeSessionParticipant.Create(session2.Id, manager1.Id));
        session2.Participants.Add(CommitteeSessionParticipant.Create(session2.Id, manager2.Id));
        session2.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session2.Id, kp1.Id));
        session2.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session2.Id, kp2.Id));
        session2.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session2.Id, kp3_old.Id));

        var session3 = CommitteeSession.Create(
            new DateOnly(prevYear, 12, 5),
            $"【{prevYear}年度 第3回 人材開発委員会（年度末振り返り）】\n\n" +
            "■ 出席者: 部長一郎、山田一郎（Manager）、鈴木二郎（Manager）\n\n" +
            "■ 年度末評価・育成振り返り\n" +
            "・田中太郎：全育成目標達成。勉強会3回開催、SSRアプリ4本リリース。来年度もキープパーソン継続。\n" +
            "・佐藤花子：最高評価（スコア5）。設計力・コミュニケーションとも大幅向上。来年度もキープパーソン継続。\n" +
            "・伊藤健太：Azure/Docker/GHAのスキルアップ目標達成。来年度は自走できると判断しキープパーソン解除。\n\n" +
            "■ 来年度方針\n" +
            $"・{currentYear}年度は田中・佐藤の2名を重点管理継続。育成方針を更新する。\n" +
            "・渡辺美咲のフロントエンド成長を来年度第1回で評価し、重点管理人物指定を検討する。\n\n" +
            "■ 次回予定\n" +
            $"・{currentYear}年5月に第1回セッションを開催予定。",
            senior.Id);
        session3.Participants.Add(CommitteeSessionParticipant.Create(session3.Id, senior.Id));
        session3.Participants.Add(CommitteeSessionParticipant.Create(session3.Id, manager1.Id));
        session3.Participants.Add(CommitteeSessionParticipant.Create(session3.Id, manager2.Id));
        session3.Participants.Add(CommitteeSessionParticipant.Create(session3.Id, exec.Id));
        session3.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session3.Id, kp1.Id));
        session3.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session3.Id, kp2.Id));
        session3.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session3.Id, kp3_old.Id));

        // 今年度 第1回セッション（先月開催済み）
        var session4 = CommitteeSession.Create(
            today.AddDays(-14),
            $"【{currentYear}年度 第1回 人材開発委員会】\n\n" +
            "■ 出席者: 部長一郎、役員太郎、山田一郎（Manager）、鈴木二郎（Manager）\n\n" +
            "■ 重点管理人物の確認\n" +
            "・昨年度から継続: 田中太郎、佐藤花子\n" +
            "・渡辺美咲について：Nuxt.js学習中で成長著しいが、まだ実績が必要。Q3で再検討。\n\n" +
            "■ 育成方針の更新\n" +
            "・田中太郎：今年度はテックリードとしてFEアーキテクチャ設計をリードさせる。\n" +
            "・佐藤花子：分散システム実装をメインで担当させ、Azureサービス活用経験を積ませる。\n\n" +
            "■ 次回予定\n" +
            $"・{currentYear}年9月に中間振り返りセッションを実施予定。",
            senior.Id);
        session4.Participants.Add(CommitteeSessionParticipant.Create(session4.Id, senior.Id));
        session4.Participants.Add(CommitteeSessionParticipant.Create(session4.Id, exec.Id));
        session4.Participants.Add(CommitteeSessionParticipant.Create(session4.Id, manager1.Id));
        session4.Participants.Add(CommitteeSessionParticipant.Create(session4.Id, manager2.Id));
        session4.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session4.Id, kp1.Id));
        session4.KeyPersons.Add(CommitteeSessionKeyPerson.Create(session4.Id, kp2.Id));

        db.CommitteeSessions.AddRange(session1, session2, session3, session4);

        // ===== 1on1面談記録 =====
        var meeting1 = OneOnOneMeeting.Create(
            talent1.Id, manager1.Id,
            new DateOnly(prevYear, 4, 10),
            MeetingFormat.Online,
            "テックリードへのキャリアパスについて話し合った。3年後のロードマップを確認。",
            "マイクロフロントエンドの学習を進めている。Nuxt.js の理解が深まっている。",
            "Q1 の OKR 達成に向けた進捗確認。アーキテクチャ設計書を Q2 末までに完成させる。",
            "体調良好。プロジェクト負荷は適度。",
            "次回は設計書レビューを予定。",
            true);

        var meeting2 = OneOnOneMeeting.Create(
            talent1.Id, manager1.Id,
            new DateOnly(prevYear, 7, 15),
            MeetingFormat.InPerson,
            "中長期キャリアの方向性を再確認。テックリードよりアーキテクトに興味があることが判明。",
            "Nuxt.js プロジェクトのリードを担当し、自信がついた。次は Azure への深い理解が必要。",
            "Q2 OKR の達成度を振り返り。アーキテクチャ設計書は完成済み。",
            "繁忙期が続いているが、モチベーションは高い。",
            null,
            true);

        var meeting3 = OneOnOneMeeting.Create(
            talent2.Id, manager1.Id,
            new DateOnly(prevYear, 5, 20),
            MeetingFormat.Online,
            "フルスタックエンジニアとして成長したい意向を確認。",
            "バックエンド（C# / ASP.NET Core）の理解が向上。",
            "スキルマップの更新が完了している。次は API 設計のレビューを担当させる。",
            "体調は問題なし。チームのコミュニケーションも良好。",
            null,
            false);

        var meeting4 = OneOnOneMeeting.Create(
            talent3.Id, manager2.Id,
            new DateOnly(prevYear, 6, 5),
            MeetingFormat.Hybrid,
            "現在のポジションに満足しているが、将来的にはマネジメントも視野に入れている。",
            "インフラ（Azure / Docker）のスキルが向上。GitHub Actions による CI/CD の自動化を担当。",
            "チームのインフラ改善プロジェクトを Q3 中に完成させる。",
            "やや疲労感があるが、プロジェクトへの貢献感は高い。",
            null,
            false);

        db.OneOnOneMeetings.AddRange(meeting1, meeting2, meeting3, meeting4);
        await db.SaveChangesAsync();

        // 宿題の追加（meeting1 の宿題）
        var item1 = MeetingActionItem.Create(meeting1.Id, "マイクロフロントエンドの概念検証（PoC）を実施する");
        var item2 = MeetingActionItem.Create(meeting1.Id, "Azure アーキテクチャ設計書の初稿を作成する");
        var item3 = MeetingActionItem.Create(meeting1.Id, "テックリードのロールディスクリプションを読み込む");

        // item1 は完了済み
        item1.Complete();

        db.MeetingActionItems.AddRange(item1, item2, item3);

        // meeting2 の宿題
        var item4 = MeetingActionItem.Create(meeting2.Id, "Azure の AZ-900 資格取得を目標として学習計画を立てる");
        var item5 = MeetingActionItem.Create(meeting2.Id, "アーキテクチャ勉強会を月次で開催する");
        item4.Complete();
        item5.Complete();
        db.MeetingActionItems.AddRange(item4, item5);

        await db.SaveChangesAsync();

        // NineBoxアセスメントは全エンティティ保存後にシード（ユーザー・期間が必要）
        await SeedNineBoxAssessmentsAsync(db);
        await SeedTalentManagerCommentsAsync(db);
        await SeedWcmEntriesAsync(db);
        await SeedCareerValuesAsync(db);
    }

    private static async Task SeedNineBoxAssessmentsAsync(AppDbContext db)
    {
        if (db.NineBoxAssessments.Any()) return;

        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");
        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");

        if (manager1 is null || manager2 is null ||
            talent1 is null || talent2 is null || talent3 is null || talent4 is null)
            return;

        var prevYear   = DateTime.UtcNow.Year - 1;
        var prevPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == prevYear);
        if (prevPeriod is null) return;

        var nb1 = NineBoxAssessment.Create(talent1.Id, prevPeriod.Id, manager1.Id,
            PerformanceLevel.High, PotentialLevel.High,
            "全目標を高いレベルで達成。SSRアプリのパフォーマンス改善は特に顕著。",
            "テックリード候補として申し分ない成長意欲と学習速度を持つ。");
        var nb2 = NineBoxAssessment.Create(talent2.Id, prevPeriod.Id, manager1.Id,
            PerformanceLevel.High, PotentialLevel.High,
            "BEタスクを全完遂。テストカバレッジ90%以上を達成した。",
            "分散システムへの強い関心と理解力から、将来のアーキテクト候補として最有力。");
        var nb3 = NineBoxAssessment.Create(talent3.Id, prevPeriod.Id, manager2.Id,
            PerformanceLevel.Mid, PotentialLevel.Mid,
            "CI/CDパイプラインの整備など担当タスクは完遂。期待値通りの業績。",
            "着実な成長を続けているが、自発的な領域拡張の意欲がやや少ない。");
        var nb4 = NineBoxAssessment.Create(talent4.Id, prevPeriod.Id, manager2.Id,
            PerformanceLevel.Low, PotentialLevel.High,
            "入社1年目でありまだ業績は限定的だが、学習意欲と吸収速度が高い。",
            "フロントエンド領域での急速な成長が期待できる。適切な育成で大化けする可能性がある。");
        db.NineBoxAssessments.AddRange(nb1, nb2, nb3, nb4);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTalentManagerCommentsAsync(AppDbContext db)
    {
        if (db.TalentManagerComments.Any()) return;

        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");

        if (manager1 is null || manager2 is null ||
            talent1 is null || talent2 is null || talent3 is null || talent4 is null)
            return;

        var currYear   = DateTime.UtcNow.Year;
        var currPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == currYear);
        if (currPeriod is null) return;

        var comments = new[]
        {
            TalentManagerComment.Create(
                talent1.Id, manager1.Id, currPeriod.Id,
                "アーキテクチャ設計の上流経験がまだ限定的。チームへの技術展開は得意だが、設計ドキュメントの精度向上が課題。",
                "新規プロダクトのFEアーキテクチャ設計をリードさせ、部長レビューを通じて設計力を底上げする。月1回の1on1で進捗を確認する。",
                "フロントエンドテックリード（2年以内）"),

            TalentManagerComment.Create(
                talent2.Id, manager1.Id, currPeriod.Id,
                "分散システムの理論理解はあるが実務経験が不足している。チーム全体への技術発信力をさらに高めてほしい。",
                "Azure Service Busを使った非同期処理基盤の実装をメインで担当させる。Q3にアーキテクチャレビューを実施し、設計力を評価する。",
                "バックエンドアーキテクト（3年以内）→ テクニカルディレクター候補"),

            TalentManagerComment.Create(
                talent3.Id, manager2.Id, currPeriod.Id,
                "インフラ領域は確立されているが、アプリケーション開発との連携スキルが不足している。",
                "インフラエンジニアとしての専門性を活かしながら、アプリ開発チームとの協業機会を増やす。Azure認定資格取得を今期の重点目標とする。",
                "クラウドインフラエンジニア（スペシャリスト）"),

            TalentManagerComment.Create(
                talent4.Id, manager2.Id, currPeriod.Id,
                "学習意欲は高く吸収速度も速いが、まだ自走で設計・実装まで完結できる実績が少ない。",
                "既存プロダクトのUI改善タスクをアサインし、フィードバックを重ねて実力をつけさせる。Nuxt.jsの実務経験を今年度中に積ませる。",
                "フロントエンドエンジニア（自走レベル到達を今年度目標）"),
        };

        db.TalentManagerComments.AddRange(comments);
        await db.SaveChangesAsync();
    }

    private static async Task SeedWcmEntriesAsync(AppDbContext db)
    {
        if (db.WillCanMustEntries.Any()) return;

        var talent1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null) return;

        var currYear   = DateTime.UtcNow.Year;
        var currPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == currYear);
        if (currPeriod is null) return;

        var skillTs    = db.SkillItems.FirstOrDefault(s => s.Name == "TypeScript");
        var skillAspnet = db.SkillItems.FirstOrDefault(s => s.Name == "ASP.NET Core");
        var skillEfcore = db.SkillItems.FirstOrDefault(s => s.Name == "Entity Framework Core");
        var skillAzure  = db.SkillItems.FirstOrDefault(s => s.Name == "Azure");
        var skillDocker = db.SkillItems.FirstOrDefault(s => s.Name == "Docker");
        var skillGha    = db.SkillItems.FirstOrDefault(s => s.Name == "GitHub Actions");

        var entries = new List<WillCanMustEntry>();

        // ── talent1（田中 太郎）：フロントエンドスペシャリスト・テックリード候補 ──
        // Will
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Will,
            "直近6ヶ月でフロントエンドのアーキテクチャ設計をリードし、チームメンバーに設計ノウハウを展開したい", 1,
            timeHorizon: WcmTimeHorizon.ShortTerm));
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Will,
            "3年以内にテックリードとして組織の技術戦略を担い、エンジニア採用・育成にも関与したい", 1,
            timeHorizon: WcmTimeHorizon.MidTerm));
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Will,
            "5年後には技術とビジネスの橋渡し役として、プロダクトの事業成長に直接貢献したい", 1,
            timeHorizon: WcmTimeHorizon.LongTerm));
        // Can - 強み
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Can,
            "TypeScriptの型設計・ジェネリクス活用が得意で、チームのコード品質向上をリードできる", 1,
            canType: CanType.Strength, linkedSkillItemId: skillTs?.Id));
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Can,
            "Vue.js / Nuxt.js を使ったSSRアプリケーションの設計・実装経験が豊富", 2,
            canType: CanType.Strength));
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Can,
            "社内勉強会の企画・運営が得意で、技術ナレッジのチーム展開が自然にできる", 3,
            canType: CanType.Strength));
        // Can - 課題
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Can,
            "アーキテクチャ設計書の作成・ドキュメント化は経験が浅く、精度向上が必要", 1,
            canType: CanType.Challenge));
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Can,
            "外部カンファレンスでの登壇経験がなく、社外への情報発信力を高めたい", 2,
            canType: CanType.Challenge));
        // Must
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Must,
            "新規プロダクトのFEアーキテクチャ設計書をQ2末までに完成させる", 1));
        entries.Add(WillCanMustEntry.Create(talent1.Id, currPeriod.Id, WcmCategory.Must,
            "月1回以上の社内技術勉強会を通年で継続し、フロントエンド技術のナレッジシェアを推進する", 2));

        // ── talent2（佐藤 花子）：バックエンドアーキテクト候補 ──
        // Will
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Will,
            "直近6ヶ月でAzure Service Busを使った非同期処理基盤の設計・実装を本番稼働まで完遂したい", 1,
            timeHorizon: WcmTimeHorizon.ShortTerm));
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Will,
            "3年以内にバックエンドアーキテクトとして社内外で認知され、大規模システム設計をリードしたい", 1,
            timeHorizon: WcmTimeHorizon.MidTerm));
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Will,
            "5年後には技術とビジネスの橋渡し役として、CTO補佐レベルの意思決定に携わりたい", 1,
            timeHorizon: WcmTimeHorizon.LongTerm));
        // Can - 強み
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Can,
            "ASP.NET CoreでのクリーンアーキテクチャAPI設計が得意。設計からテストまで一人で完結できる", 1,
            canType: CanType.Strength, linkedSkillItemId: skillAspnet?.Id));
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Can,
            "EF Coreのマイグレーション・クエリ最適化に精通しており、パフォーマンスチューニングができる", 2,
            canType: CanType.Strength, linkedSkillItemId: skillEfcore?.Id));
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Can,
            "テスト駆動開発の実践経験があり、テストカバレッジ90%以上を維持できる", 3,
            canType: CanType.Strength));
        // Can - 課題
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Can,
            "分散システム・イベント駆動アーキテクチャの理論理解はあるが実務経験が少ない", 1,
            canType: CanType.Challenge));
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Can,
            "プロジェクト全体のコミュニケーションリードや、関係者への技術説明力を高めたい", 2,
            canType: CanType.Challenge));
        // Must
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Must,
            "次期システムのBEアーキテクチャ設計書をQ3末までに完成させ、チームレビューを通過させる", 1));
        entries.Add(WillCanMustEntry.Create(talent2.Id, currPeriod.Id, WcmCategory.Must,
            "Azure Service Busを活用した非同期処理基盤を設計・実装し本番稼働まで担当する", 2));

        // ── talent3（伊藤 健太）：インフラエンジニア ──
        // Will
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Will,
            "今年度中にAzure Solutions Architect Expert資格を取得したい", 1,
            timeHorizon: WcmTimeHorizon.ShortTerm));
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Will,
            "クラウドインフラの設計をリードするスペシャリストとして組織内で認められたい", 1,
            timeHorizon: WcmTimeHorizon.MidTerm));
        // Can - 強み
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Can,
            "Azure環境の設計・構築・運用の実務経験が豊富で、コスト最適化の提案ができる", 1,
            canType: CanType.Strength, linkedSkillItemId: skillAzure?.Id));
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Can,
            "Dockerを使ったコンテナ化・マルチステージビルドの経験があり、本番環境での運用実績がある", 2,
            canType: CanType.Strength, linkedSkillItemId: skillDocker?.Id));
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Can,
            "GitHub ActionsによるCI/CDパイプライン構築の経験があり、デプロイ自動化を推進できる", 3,
            canType: CanType.Strength, linkedSkillItemId: skillGha?.Id));
        // Can - 課題
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Can,
            "アプリケーション開発チームとの協業経験が少なく、開発者目線のインフラ設計が課題", 1,
            canType: CanType.Challenge));
        // Must
        entries.Add(WillCanMustEntry.Create(talent3.Id, currPeriod.Id, WcmCategory.Must,
            "CI/CDパイプラインの全面刷新を担当し、デプロイ自動化率80%以上を達成する", 1));

        // ── talent4（渡辺 美咲）：フロントエンドエンジニア（成長中） ──
        // Will
        entries.Add(WillCanMustEntry.Create(talent4.Id, currPeriod.Id, WcmCategory.Will,
            "半年以内にNuxt.jsを使った実務案件を自走で完結できるレベルになりたい", 1,
            timeHorizon: WcmTimeHorizon.ShortTerm));
        entries.Add(WillCanMustEntry.Create(talent4.Id, currPeriod.Id, WcmCategory.Will,
            "フロントエンドエンジニアとして独力でUI設計・実装ができるレベルに成長し、チームに貢献したい", 1,
            timeHorizon: WcmTimeHorizon.MidTerm));
        // Can - 強み
        entries.Add(WillCanMustEntry.Create(talent4.Id, currPeriod.Id, WcmCategory.Can,
            "Vue.jsの基礎は習得済みで、コンポーネント設計の基本が理解できている", 1,
            canType: CanType.Strength));
        entries.Add(WillCanMustEntry.Create(talent4.Id, currPeriod.Id, WcmCategory.Can,
            "学習意欲が高く、新しい技術の吸収速度が速い", 2,
            canType: CanType.Strength));
        // Can - 課題
        entries.Add(WillCanMustEntry.Create(talent4.Id, currPeriod.Id, WcmCategory.Can,
            "実務での設計から実装まで自走で完結した経験がまだ少なく、経験を積む必要がある", 1,
            canType: CanType.Challenge));
        // Must
        entries.Add(WillCanMustEntry.Create(talent4.Id, currPeriod.Id, WcmCategory.Must,
            "既存プロダクトのUI改善タスクを自走で担当し、レビューを通過させる", 1));

        db.WillCanMustEntries.AddRange(entries);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCareerValuesAsync(AppDbContext db)
    {
        if (db.CareerValues.Any()) return;

        var talent1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null) return;

        var currYear   = DateTime.UtcNow.Year;
        var currPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == currYear);
        if (currPeriod is null) return;

        var values = new[]
        {
            CareerValues.Create(
                talent1.Id, currPeriod.Id,
                "チームの成長と技術的な誠実さを大切にする。良いコードは次の開発者へのプレゼントだと考えている。",
                "難しい技術課題をチームで議論し、解決策を見つけてプロダクトをリリースできた瞬間。個人の成果より、チームとして動けたときに最も充実感を感じる。",
                "技術的負債が積み上がることを分かっていながら、スピードだけを優先させられる状況。コードの品質や設計の議論が軽視される文化。"),

            CareerValues.Create(
                talent2.Id, currPeriod.Id,
                "高品質なコードで、未来の開発者への責任を果たすこと。設計の美しさと実装の堅牢さを両立することを重視する。",
                "自分が設計したアーキテクチャが本番環境でスケールし、ユーザーに価値を届けていると実感できるとき。複雑な問題をシンプルな構造で解決できたときの達成感。",
                "品質よりスピードだけが評価される状況。また、技術的な意思決定に関われず、ただ言われたものを実装するだけの役割に留まること。"),

            CareerValues.Create(
                talent3.Id, currPeriod.Id,
                "システムの安定性と信頼性を守ること。インフラが安定してこそ、開発者が安心して機能開発に集中できると考えている。",
                "自分が構築したインフラが障害なく稼働し、チームが自信を持ってリリースできている状態を維持できているとき。",
                "場当たり的な対応でインフラが複雑化し、技術的負債が蓄積していく状況。可用性・セキュリティよりコスト削減だけが優先される意思決定。"),

            CareerValues.Create(
                talent4.Id, currPeriod.Id,
                "ユーザーが使いやすいUIを作ること。見た目の美しさだけでなく、直感的に操作できる体験を大切にしたい。",
                "自分が作ったUIをユーザーが使っている場面を見るとき。先輩からのコードレビューで新しいことを学べたとき。",
                "ユーザー体験を考慮せず、技術的な理由だけで設計が決まる状況。学習の機会が少なく、同じ作業の繰り返しで成長を感じられない環境。"),
        };

        db.CareerValues.AddRange(values);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCellDefinitionsAsync(AppDbContext db)
    {
        if (db.NineBoxCellDefinitions.Any()) return;

        var cells = new[]
        {
            NineBoxCellDefinition.Create(PerformanceLevel.High, PotentialLevel.High, "スター",     "業績・ポテンシャルともに最高水準。組織の中核として即戦力かつ将来リーダー候補。", "#8b5cf6"),
            NineBoxCellDefinition.Create(PerformanceLevel.High, PotentialLevel.Mid,  "高業績者",   "現時点での業績は最高水準。成長の余地はあるが組織への貢献度が高い。",           "#6366f1"),
            NineBoxCellDefinition.Create(PerformanceLevel.High, PotentialLevel.Low,  "専門家",     "専門領域で卓越した業績を持つが、ポテンシャルは現ポジションに特化している。",   "#3b82f6"),
            NineBoxCellDefinition.Create(PerformanceLevel.Mid,  PotentialLevel.High, "原石",       "高いポテンシャルを持ち、将来の活躍が期待できる。業績向上に向けた育成が重要。", "#10b981"),
            NineBoxCellDefinition.Create(PerformanceLevel.Mid,  PotentialLevel.Mid,  "コア人材",   "業績・ポテンシャルともに安定した中堅層。組織の安定的な基盤を担う。",             "#6b7280"),
            NineBoxCellDefinition.Create(PerformanceLevel.Mid,  PotentialLevel.Low,  "安定人材",   "現在のポジションで安定した業績を発揮。成長よりも定着・安定運用を優先。",         "#9ca3af"),
            NineBoxCellDefinition.Create(PerformanceLevel.Low,  PotentialLevel.High, "将来有望",   "現時点では業績が低いが高いポテンシャルがあり、適切な育成で大きく成長する可能性。","#f59e0b"),
            NineBoxCellDefinition.Create(PerformanceLevel.Low,  PotentialLevel.Mid,  "要育成",     "業績・ポテンシャルともに改善の余地あり。明確な育成計画と継続的サポートが必要。", "#f97316"),
            NineBoxCellDefinition.Create(PerformanceLevel.Low,  PotentialLevel.Low,  "要検討",     "業績・ポテンシャルともに低い。配置転換・役割変更等を含めた抜本的な検討が必要。", "#ef4444"),
        };
        db.NineBoxCellDefinitions.AddRange(cells);
        await db.SaveChangesAsync();
    }
}
