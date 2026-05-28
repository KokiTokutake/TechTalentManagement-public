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
            await SeedSkillLevelDefinitionsAsync(db);
            await SeedCompetencyMasterAsync(db);
            await SeedCompetencyEvaluationsAsync(db);
            await SeedGoalKpisAndCheckinsAsync(db);
            await SeedFeedbackMeetingsAsync(db);
            await SeedCriticalPositionsAsync(db);
            await SeedProjectAssignmentsAsync(db);
            await SeedMdcDecisionsAsync(db);
            await SeedNotificationsAsync(db);
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
        await SeedSkillLevelDefinitionsAsync(db);
        await SeedCompetencyMasterAsync(db);
        await SeedCompetencyEvaluationsAsync(db);
        await SeedGoalKpisAndCheckinsAsync(db);
        await SeedFeedbackMeetingsAsync(db);
        await SeedCriticalPositionsAsync(db);
        await SeedProjectAssignmentsAsync(db);
        await SeedMdcDecisionsAsync(db);
        await SeedNotificationsAsync(db);
    }

    private static async Task SeedNineBoxAssessmentsAsync(AppDbContext db)
    {
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");
        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");

        if (manager1 is null || manager2 is null ||
            talent1 is null || talent2 is null || talent3 is null || talent4 is null)
            return;

        var currYear   = DateTime.UtcNow.Year;
        var prevYear   = currYear - 1;
        var prevPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == prevYear);
        var currPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == currYear);

        // 前年度（2025）アセスメント
        if (prevPeriod is not null && !db.NineBoxAssessments.Any(n => n.EvaluationPeriodId == prevPeriod.Id))
        {
            db.NineBoxAssessments.AddRange(
                NineBoxAssessment.Create(talent1.Id, prevPeriod.Id, manager1.Id,
                    PerformanceLevel.High, PotentialLevel.High,
                    "全目標を高いレベルで達成。SSRアプリのパフォーマンス改善は特に顕著。",
                    "テックリード候補として申し分ない成長意欲と学習速度を持つ。"),
                NineBoxAssessment.Create(talent2.Id, prevPeriod.Id, manager1.Id,
                    PerformanceLevel.High, PotentialLevel.High,
                    "BEタスクを全完遂。テストカバレッジ90%以上を達成した。",
                    "分散システムへの強い関心と理解力から、将来のアーキテクト候補として最有力。"),
                NineBoxAssessment.Create(talent3.Id, prevPeriod.Id, manager2.Id,
                    PerformanceLevel.Mid, PotentialLevel.Mid,
                    "CI/CDパイプラインの整備など担当タスクは完遂。期待値通りの業績。",
                    "着実な成長を続けているが、自発的な領域拡張の意欲がやや少ない。"),
                NineBoxAssessment.Create(talent4.Id, prevPeriod.Id, manager2.Id,
                    PerformanceLevel.Low, PotentialLevel.High,
                    "入社1年目でありまだ業績は限定的だが、学習意欲と吸収速度が高い。",
                    "フロントエンド領域での急速な成長が期待できる。適切な育成で大化けする可能性がある。")
            );
            await db.SaveChangesAsync();
        }

        // 今年度（2026）アセスメント — 前半期終了後の中間評価として入力済み
        if (currPeriod is not null && !db.NineBoxAssessments.Any(n => n.EvaluationPeriodId == currPeriod.Id))
        {
            db.NineBoxAssessments.AddRange(
                NineBoxAssessment.Create(talent1.Id, currPeriod.Id, manager1.Id,
                    PerformanceLevel.High, PotentialLevel.High,
                    "FEアーキテクチャ設計書をQ1で完成。設計品質が大幅に向上し、チームへの技術共有も継続できている。",
                    "テックリードへの移行ペースが加速。来年度にはリード業務を正式に任せられる段階に来た。"),
                NineBoxAssessment.Create(talent2.Id, currPeriod.Id, manager1.Id,
                    PerformanceLevel.High, PotentialLevel.High,
                    "Azure Service Bus導入を主導し、非同期処理基盤を本番稼働まで完遂した。設計書品質も向上。",
                    "分散システム実装の実績ができた。アーキテクト候補としての実力が証明されつつある。"),
                NineBoxAssessment.Create(talent3.Id, currPeriod.Id, manager2.Id,
                    PerformanceLevel.Mid, PotentialLevel.Mid,
                    "CI/CDパイプライン刷新が順調に進行中。Azure認定資格の学習も継続している。",
                    "安定した実力があるが、自発的に新領域へ踏み出す動きがまだ少ない。引き続き機会を作っていく。"),
                NineBoxAssessment.Create(talent4.Id, currPeriod.Id, manager2.Id,
                    PerformanceLevel.Low, PotentialLevel.High,
                    "UI改善タスクを自走で完遂。Nuxt.jsの習得速度は想定以上で、品質も許容範囲内。",
                    "成長スピードが昨年より加速している。来年度は単独でのUI設計・実装を任せられる可能性が高い。")
            );
            await db.SaveChangesAsync();
        }
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

    // ── スキルレベル定義 ──────────────────────────────────────────────────────
    private static async Task SeedSkillLevelDefinitionsAsync(AppDbContext db)
    {
        if (db.SkillLevelDefinitions.Any()) return;

        var skills = db.SkillItems.ToList();
        if (!skills.Any()) return;

        var defs = new List<SkillLevelDefinition>();

        var levelTexts = new Dictionary<string, string[]>
        {
            ["TypeScript"] = [
                "型の基礎（string/number/boolean/any）を理解し、型アノテーションが書ける",
                "インターフェース・型エイリアス・ユニオン型・ジェネリクス基礎を使いこなせる",
                "ジェネリクス応用・Mapped Types・Conditional Types を活用したコードが書ける",
                "型安全な設計をチーム全体に展開でき、コードレビューで型の問題を指摘できる",
                "TypeScript の型システムを深く理解し、ライブラリの型定義や複雑な型パズルを解決できる"
            ],
            ["Vue.js"] = [
                "Options API を使ったコンポーネント作成・props/emit の基礎が分かる",
                "Composition API・ref/reactive・computed を使いこなせる",
                "Pinia による状態管理・カスタムコンポーザブル設計ができる",
                "大規模 SPA の設計・パフォーマンス最適化（Virtual DOM, keepAlive）ができる",
                "Vue エコシステム全体（プラグイン・SSR・テスト戦略）を設計・実装できる"
            ],
            ["Nuxt.js"] = [
                "Nuxt の基本構成（pages/layouts/components）を理解しページが作れる",
                "SSR/SSG の違いを理解し、useAsyncData・useFetch を適切に使い分けられる",
                "ミドルウェア・プラグイン・Nitro サーバーAPI を使った機能実装ができる",
                "大規模 Nuxt アプリのアーキテクチャ設計・最適化・デプロイ戦略ができる",
                "Nuxt モジュール開発・ランタイム設定・カスタム Nitro プラグインまで対応できる"
            ],
            ["C#"] = [
                "基本構文・クラス・インターフェース・LINQ 基礎が書ける",
                "async/await・ジェネリクス・拡張メソッド・デリゲートを適切に使える",
                "DI・設計パターン（Repository, CQRS等）を用いた設計・実装ができる",
                "アーキテクチャ設計からテストまで一人でリードできる",
                "C# の内部動作（IL, GC, メモリモデル）を理解しパフォーマンスチューニングができる"
            ],
            ["ASP.NET Core"] = [
                "基本的な Web API コントローラーを作成しエンドポイントを定義できる",
                "ミドルウェア・フィルター・モデルバインディング・バリデーションを使いこなせる",
                "認証（JWT/Cookie）・認可・CORS・ロギング設定ができる",
                "クリーンアーキテクチャで大規模 API を設計・実装・テストできる",
                "ASP.NET Core のパイプライン・ホスティングモデルを深く理解し最適化できる"
            ],
            ["Entity Framework Core"] = [
                "DbContext・マイグレーション・基本的な CRUD クエリが書ける",
                "リレーション（HasMany/HasOne）・Include・遅延/即時ロードを使いこなせる",
                "インデックス設計・クエリ最適化・N+1問題の解消ができる",
                "Bulk 操作・バッチ処理・複雑なクエリの最適化ができる",
                "EF Core の内部動作（Change Tracker, SQL生成）を理解しパフォーマンス設計ができる"
            ],
            ["Azure"] = [
                "Azure ポータルでの基本操作・Virtual Machine・Storage の作成ができる",
                "App Service・Azure SQL・Container Apps の構築・デプロイができる",
                "ネットワーク設計（VNet/NSG/Private Endpoint）・Key Vault・Monitor の運用ができる",
                "マルチリージョン構成・コスト最適化・セキュリティ強化設計ができる",
                "大規模エンタープライズアーキテクチャを Azure 上で設計・実装・運用できる"
            ],
            ["Docker"] = [
                "Dockerfile の基礎・イメージのビルド・コンテナの起動ができる",
                "マルチステージビルド・docker compose での複数サービス構成ができる",
                "本番環境でのコンテナ運用・ネットワーク設計・ボリューム管理ができる",
                "コンテナレジストリ・CI/CD 連携・セキュリティスキャンができる",
                "Kubernetes 連携も含めたコンテナ基盤の設計・運用ができる"
            ],
            ["GitHub Actions"] = [
                "基本的な workflow の作成・push/PR トリガーのパイプラインが書ける",
                "matrix build・secrets 管理・アーティファクト保存ができる",
                "デプロイパイプライン（Staging/Production）・承認フロー設計ができる",
                "再利用可能な composite action・reusable workflow の設計・実装ができる",
                "大規模組織向けの Actions 基盤設計・self-hosted runner 管理ができる"
            ],
        };

        var examples = new Dictionary<string, string[]>
        {
            ["TypeScript"] = [
                "Hello World, 簡単な計算関数の型付け",
                "React/Vue コンポーネントの型定義",
                "API レスポンス型の定義・型安全な fetch ラッパー",
                "チーム全体の型定義レビュー担当",
                "型定義ライブラリの作成・複雑な型体操"
            ],
            ["Vue.js"] = [
                "静的ページのコンポーネント化",
                "フォーム・一覧ページの実装",
                "ストア設計を含む SPA 実装",
                "パフォーマンス改善・アーキテクチャ設計",
                "OSS コントリビュート・社内フレームワーク構築"
            ],
            ["Nuxt.js"] = [
                "静的サイトの作成",
                "SSR アプリの実装",
                "認証付き SSR アプリ・API Routes 実装",
                "大規模 SSR アプリのアーキテクチャ",
                "Nuxt モジュール開発"
            ],
            ["C#"] = [
                "コンソールアプリ・基本クラス設計",
                "非同期処理を含む機能実装",
                "DI コンテナを使ったアーキテクチャ設計",
                "プロジェクト全体のアーキテクチャ設計",
                "メモリ最適化・高スループットシステム実装"
            ],
            ["ASP.NET Core"] = [
                "CRUD API の実装",
                "バリデーション・エラーハンドリングの実装",
                "JWT 認証・ロールベース認可の実装",
                "Clean Architecture での大規模 API 設計",
                "フレームワーク拡張・パフォーマンスチューニング"
            ],
            ["Entity Framework Core"] = [
                "単純なテーブルの CRUD",
                "リレーションを含む CRUD 操作",
                "複雑なクエリ最適化",
                "バルク操作・大量データ処理",
                "EF Core のカスタマイズ・パフォーマンス設計"
            ],
            ["Azure"] = [
                "Azure ポータルでの VM 作成",
                "Web アプリのデプロイ・DB 接続",
                "本番環境の構築・監視設定",
                "マルチリージョン・DR 設計",
                "エンタープライズ Landing Zone 設計"
            ],
            ["Docker"] = [
                "開発環境の Docker 化",
                "マルチコンテナ開発環境の構築",
                "本番コンテナ環境の運用",
                "CI/CD パイプラインへの組み込み",
                "Kubernetes クラスター設計"
            ],
            ["GitHub Actions"] = [
                "PR 時の自動テスト実行",
                "マルチプラットフォームビルド",
                "ステージング・本番デプロイパイプライン",
                "組織共通ワークフローの作成",
                "大規模 CI/CD 基盤の設計・運用"
            ],
        };

        foreach (var skill in skills)
        {
            if (!levelTexts.TryGetValue(skill.Name, out var texts)) continue;
            if (!examples.TryGetValue(skill.Name, out var exs)) continue;
            for (int lv = 1; lv <= 5; lv++)
            {
                defs.Add(SkillLevelDefinition.Create(skill.Id, lv, texts[lv - 1], exs[lv - 1]));
            }
        }

        db.SkillLevelDefinitions.AddRange(defs);
        await db.SaveChangesAsync();
    }

    // ── コンピテンシーマスター ─────────────────────────────────────────────────
    private static async Task SeedCompetencyMasterAsync(AppDbContext db)
    {
        if (db.CompetencyMasters.Any()) return;

        var masters = new[]
        {
            // Technology
            CompetencyMaster.Create(CompetencyCategory.Technology, "クラウドアーキテクチャ設計",
                "Azure等のクラウドサービスを活用し、スケーラブルで可用性の高いシステムを設計できる能力", 1),
            CompetencyMaster.Create(CompetencyCategory.Technology, "セキュリティ設計・実装",
                "認証・認可・脆弱性対策を含むセキュアなシステムを設計・実装できる能力", 2),
            CompetencyMaster.Create(CompetencyCategory.Technology, "パフォーマンス最適化",
                "ボトルネック分析・クエリ最適化・キャッシュ戦略等でシステム性能を改善できる能力", 3),
            CompetencyMaster.Create(CompetencyCategory.Technology, "テスト設計・品質保証",
                "単体・結合・E2Eテストを含む品質保証の仕組みを設計・実践できる能力", 4),
            // Methodology
            CompetencyMaster.Create(CompetencyCategory.Methodology, "アジャイル開発実践",
                "スクラム・カンバン等のアジャイル手法を理解し、チームとして実践できる能力", 1),
            CompetencyMaster.Create(CompetencyCategory.Methodology, "コードレビュー",
                "建設的なフィードバックでチームのコード品質向上に貢献できる能力", 2),
            CompetencyMaster.Create(CompetencyCategory.Methodology, "技術的意思決定",
                "技術選定・アーキテクチャ判断をトレードオフを踏まえて行える能力", 3),
            // BusinessIndustry
            CompetencyMaster.Create(CompetencyCategory.BusinessIndustry, "金融系業務知識",
                "金融業界の規制・コンプライアンス要件を理解しシステム設計に反映できる能力", 1),
            CompetencyMaster.Create(CompetencyCategory.BusinessIndustry, "要件定義・仕様化",
                "ステークホルダーのニーズを技術仕様に落とし込める能力", 2),
            // Common
            CompetencyMaster.Create(CompetencyCategory.Common, "コミュニケーション",
                "技術的な内容を相手のレベルに合わせてわかりやすく説明できる能力", 1),
            CompetencyMaster.Create(CompetencyCategory.Common, "チームワーク・協働",
                "チームメンバーと協力し、目標達成に向けて主体的に貢献できる能力", 2),
            CompetencyMaster.Create(CompetencyCategory.Common, "問題解決力",
                "複雑な問題を構造化し、効果的な解決策を導ける能力", 3),
            CompetencyMaster.Create(CompetencyCategory.Common, "リーダーシップ・育成",
                "チームや後輩を引っ張り、他者の成長を支援できる能力", 4),
        };

        db.CompetencyMasters.AddRange(masters);
        await db.SaveChangesAsync();
    }

    // ── コンピテンシー評価 ────────────────────────────────────────────────────
    private static async Task SeedCompetencyEvaluationsAsync(AppDbContext db)
    {
        if (db.CompetencyEvaluations.Any()) return;

        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null
            || manager1 is null || manager2 is null) return;

        var currYear   = DateTime.UtcNow.Year;
        var prevYear   = currYear - 1;
        var currPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == currYear);
        var prevPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == prevYear);
        var masters    = db.CompetencyMasters.ToList();
        if (!masters.Any() || currPeriod is null || prevPeriod is null) return;

        var evals = new List<CompetencyEvaluation>();

        // ヘルパー: (self, manager) スコアの組で評価を生成
        void AddEvals(Guid talentId, Guid managerId, Guid periodId,
            Dictionary<string, (int self, int mgr, string? selfComment, string? mgrComment)> scores)
        {
            foreach (var master in masters)
            {
                if (!scores.TryGetValue(master.Name, out var s)) continue;
                evals.Add(CompetencyEvaluation.Create(talentId, talentId, master.Id, periodId,
                    s.self, s.selfComment, CompetencyEvaluatorType.Self));
                evals.Add(CompetencyEvaluation.Create(talentId, managerId, master.Id, periodId,
                    s.mgr, s.mgrComment, CompetencyEvaluatorType.Manager));
            }
        }

        // ── 前年度 ──
        AddEvals(talent1.Id, manager1.Id, prevPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (2, 2, "AzureはFEデプロイ程度。今後強化したい", "FE専門なのでレベル2が妥当"),
            ["セキュリティ設計・実装"]       = (3, 3, "JWT認証・CORS設定の実装経験あり", "APIセキュリティの基礎は押さえている"),
            ["パフォーマンス最適化"]         = (4, 4, "Core Web Vitals改善で顕著な成果を出した", "FE最適化については社内トップクラス"),
            ["テスト設計・品質保証"]         = (3, 3, "Vitestでのユニットテストを実践している", "テストカバレッジ意識は高い"),
            ["アジャイル開発実践"]           = (4, 4, "スプリント全Sprint完遂、チームの模範", "スクラムへの貢献度が高い"),
            ["コードレビュー"]               = (4, 4, "丁寧で建設的なレビューができている", "レビュー品質はチーム内でも高評価"),
            ["技術的意思決定"]               = (3, 3, "技術選定の議論に積極的に参加している", "もう少し上流から関わってほしい"),
            ["金融系業務知識"]               = (2, 2, "業務知識はまだ浅い", "エンジニア観点での業務理解は十分"),
            ["要件定義・仕様化"]             = (3, 3, "仕様書の作成・レビューに貢献できている", "仕様の具体化力は成長している"),
            ["コミュニケーション"]           = (4, 4, "社内勉強会を3回開催。発信力が高い", "技術コミュニケーション能力は優秀"),
            ["チームワーク・協働"]           = (4, 4, "チームメンバーのサポートを率先している", "チームへの貢献度が非常に高い"),
            ["問題解決力"]                   = (4, 4, "SSRパフォーマンス課題を独力で解決した", "問題の本質を捉えた解決策を出せる"),
            ["リーダーシップ・育成"]         = (3, 3, "勉強会開催でチームの底上げに貢献", "リーダーシップを発揮しつつある"),
        });

        AddEvals(talent2.Id, manager1.Id, prevPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (3, 3, "Azure Container Apps, SQL の構成経験あり", "インフラ理解は標準以上"),
            ["セキュリティ設計・実装"]       = (4, 4, "認証・認可・脆弱性対策の実装を主導した", "セキュリティ設計の実力は高い"),
            ["パフォーマンス最適化"]         = (4, 5, "EFCoreクエリ最適化でレスポンス改善を達成", "BEパフォーマンス最適化は社内トップ"),
            ["テスト設計・品質保証"]         = (5, 5, "テストカバレッジ90%以上を達成・維持している", "テスト設計の実力は突出している"),
            ["アジャイル開発実践"]           = (4, 4, "スプリント管理・タスク分解が的確", "アジャイルの理解と実践が高いレベル"),
            ["コードレビュー"]               = (4, 4, "設計観点まで踏み込んだレビューができる", "コードレビューの質が高い"),
            ["技術的意思決定"]               = (4, 4, "アーキテクチャ選択の根拠を明確に説明できる", "技術判断の根拠を丁寧に説明できる"),
            ["金融系業務知識"]               = (2, 2, "業務知識の習得が課題", "技術力に業務知識が追いついていない"),
            ["要件定義・仕様化"]             = (4, 4, "設計書を丁寧に作成し、レビューも通過している", "仕様化能力は大幅に向上した"),
            ["コミュニケーション"]           = (3, 3, "チーム内はよいが、外部への発信力を高めたい", "社外発信の機会を作っていきたい"),
            ["チームワーク・協働"]           = (4, 4, "チームのBE技術をリードしている", "チームへの技術貢献が大きい"),
            ["問題解決力"]                   = (5, 5, "複雑な設計課題を単独で解決できる", "問題解決能力は非常に高い"),
            ["リーダーシップ・育成"]         = (3, 3, "技術的なメンタリングを始めている", "リーダーシップの発揮に期待している"),
        });

        AddEvals(talent3.Id, manager2.Id, prevPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (3, 3, "Azure の設計・運用経験が豊富", "インフラ領域では信頼できる"),
            ["セキュリティ設計・実装"]       = (3, 3, "NSG・Key Vault 設定の経験あり", "セキュリティ基礎は押さえている"),
            ["パフォーマンス最適化"]         = (3, 3, "インフラレベルの最適化経験あり", "アプリ側の最適化はこれから"),
            ["テスト設計・品質保証"]         = (2, 2, "インフラのテストは行っているが体系的ではない", "テスト手法の習得が必要"),
            ["アジャイル開発実践"]           = (3, 3, "スクラムの基礎は理解している", "もう少し積極的な参画を期待"),
            ["コードレビュー"]               = (2, 2, "IaC レビューは行っているがアプリ側は難しい", "アプリ開発チームとの協働が課題"),
            ["技術的意思決定"]               = (3, 3, "インフラ技術選定の根拠を説明できる", "アプリ側との連携観点が課題"),
            ["金融系業務知識"]               = (2, 2, "業務知識はまだ発展途上", "インフラ寄りのため業務知識習得は後回しに"),
            ["要件定義・仕様化"]             = (2, 2, "インフラ要件の整理はできるがアプリ要件は難しい", "要件定義への参画を増やしてほしい"),
            ["コミュニケーション"]           = (3, 3, "技術的な説明は丁寧にできている", "他部門へのコミュニケーションも期待"),
            ["チームワーク・協働"]           = (3, 3, "インフラ担当としてチームをサポートしている", "アプリチームとの連携を強化してほしい"),
            ["問題解決力"]                   = (3, 3, "インフラ障害対応で的確な問題解決ができる", "問題解決力は安定している"),
            ["リーダーシップ・育成"]         = (2, 2, "インフラ分野での指導経験がまだ少ない", "後輩育成への参画を期待している"),
        });

        AddEvals(talent4.Id, manager2.Id, prevPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (1, 1, "まだ学習中", "これからの習得に期待"),
            ["セキュリティ設計・実装"]       = (1, 1, "基礎知識の習得中", "セキュリティ意識は持っている"),
            ["パフォーマンス最適化"]         = (1, 2, "最適化の概念は理解している", "UI改善でパフォーマンス意識は持っている"),
            ["テスト設計・品質保証"]         = (2, 2, "ユニットテストの基礎は学んでいる", "テスト習慣の定着を期待"),
            ["アジャイル開発実践"]           = (2, 3, "スクラムへの参加姿勢が良い", "新人として積極的にスクラムに参加している"),
            ["コードレビュー"]               = (2, 2, "コードレビューを受ける側として成長中", "レビュー指摘を真摯に受け止めている"),
            ["技術的意思決定"]               = (1, 1, "まだ技術判断の経験が少ない", "技術判断の経験を積んでいく段階"),
            ["金融系業務知識"]               = (1, 1, "業務知識は学習中", "入社1年目なので許容範囲"),
            ["要件定義・仕様化"]             = (1, 1, "仕様理解の段階", "要件理解力は伸びている"),
            ["コミュニケーション"]           = (3, 3, "積極的に質問・相談ができている", "コミュニケーション姿勢は良好"),
            ["チームワーク・協働"]           = (3, 3, "チームに馴染み、協力的な姿勢がある", "チームへの融合は順調"),
            ["問題解決力"]                   = (2, 2, "簡単な問題は解決できるようになってきた", "成長している。引き続き経験を積んでほしい"),
            ["リーダーシップ・育成"]         = (1, 1, "まだリーダーシップの発揮は難しい段階", "まずは自走できるレベルを目指してほしい"),
        });

        // ── 今年度 ──
        AddEvals(talent1.Id, manager1.Id, currPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (3, 3, "Azure Container AppsへのデプロイをリードするようになってきたFE設計との連携を理解できている", "FE‐インフラ連携の設計判断ができるようになった"),
            ["セキュリティ設計・実装"]       = (3, 4, "認証フローの設計を主導できた", "セキュリティ設計の実力が上がっている"),
            ["パフォーマンス最適化"]         = (5, 5, "マイクロFE分割でバンドルサイズを30%削減した", "FEパフォーマンス最適化は社内最高水準"),
            ["テスト設計・品質保証"]         = (4, 4, "E2EテストをPlaywrightで整備し品質基準を確立した", "テスト設計力が向上している"),
            ["アジャイル開発実践"]           = (4, 4, "スプリントゴールの設定・振り返りを率先して行っている", "アジャイルへの貢献が高い"),
            ["コードレビュー"]               = (5, 4, "設計・型安全性・パフォーマンスの観点でレビューができる", "レビュー品質は非常に高い"),
            ["技術的意思決定"]               = (4, 4, "FEアーキテクチャ選定を根拠を持って主導できた", "テックリードとしての技術判断力が育っている"),
            ["金融系業務知識"]               = (2, 3, "業務要件の理解が少しずつ深まってきた", "システム設計に業務知識を反映できるようになった"),
            ["要件定義・仕様化"]             = (4, 4, "仕様書の作成・レビューをリードできている", "仕様化能力が実用レベルに達した"),
            ["コミュニケーション"]           = (5, 5, "社内勉強会を継続。社外登壇も達成した", "技術発信力が組織内で際立っている"),
            ["チームワーク・協働"]           = (4, 5, "チームの技術底上げに積極的に貢献している", "チームへの影響力が大きくなってきた"),
            ["問題解決力"]                   = (4, 4, "アーキテクチャ課題を構造化して解決できた", "問題解決プロセスが洗練されてきた"),
            ["リーダーシップ・育成"]         = (4, 4, "後輩エンジニアのメンタリングを開始した", "リーダーシップが実際の行動に現れてきた"),
        });

        AddEvals(talent2.Id, manager1.Id, currPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (4, 4, "Azure Service Busを使った非同期処理基盤を本番稼働まで設計・実装した", "クラウドアーキテクチャの実力が証明された"),
            ["セキュリティ設計・実装"]       = (4, 5, "認証・認可の設計と実装を主導できた", "セキュリティ設計の実力は社内トップクラス"),
            ["パフォーマンス最適化"]         = (4, 5, "Service Bus導入で非同期処理のスループットを大幅改善した", "システム全体のパフォーマンス設計ができる"),
            ["テスト設計・品質保証"]         = (5, 5, "テストカバレッジ90%以上の維持に加えE2Eテストも整備した", "テスト設計の実力は突出している"),
            ["アジャイル開発実践"]           = (4, 4, "スプリント管理・見積もりの精度が上がっている", "アジャイルの模範的な実践者"),
            ["コードレビュー"]               = (5, 5, "アーキテクチャレベルのレビューができる", "コードレビューの質はチームで最高水準"),
            ["技術的意思決定"]               = (4, 5, "分散システムの技術選定を根拠を持って主導した", "技術的意思決定の実力が証明された"),
            ["金融系業務知識"]               = (3, 3, "業務要件をシステム設計に反映できるようになった", "業務とシステムの橋渡しができるようになってきた"),
            ["要件定義・仕様化"]             = (4, 5, "設計書をリードし、チームレビューを通過させている", "要件定義・仕様化の実力は高い"),
            ["コミュニケーション"]           = (3, 4, "技術説明力が向上。ステークホルダーへの説明ができる", "コミュニケーション能力が上がってきた"),
            ["チームワーク・協働"]           = (4, 4, "BEチームをリードし、FEチームとも連携している", "フルスタックとしてのチーム貢献が大きい"),
            ["問題解決力"]                   = (5, 5, "複雑な分散システムの問題を独力で解決できた", "問題解決能力は非常に高い"),
            ["リーダーシップ・育成"]         = (4, 4, "後輩のコードレビューを通じた育成が始まっている", "リーダーシップが実際の行動に現れてきた"),
        });

        AddEvals(talent3.Id, manager2.Id, currPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (4, 3, "Azure認定資格の学習でアーキテクチャ知識が体系化された", "知識は増えているが実践機会をさらに増やしたい"),
            ["セキュリティ設計・実装"]       = (3, 3, "インフラセキュリティの実務経験が積まれている", "セキュリティ基礎は安定している"),
            ["パフォーマンス最適化"]         = (3, 3, "CI/CDパイプライン高速化を実現した", "インフラパフォーマンスの改善ができる"),
            ["テスト設計・品質保証"]         = (3, 3, "インフラコードのテストを整備しつつある", "IaCのテスト習慣が定着してきた"),
            ["アジャイル開発実践"]           = (3, 3, "スクラムへの参加姿勢が向上している", "アジャイルへの参画が積極的になってきた"),
            ["コードレビュー"]               = (3, 3, "インフラコードのレビューができるようになった", "インフラコードのレビュー力が向上している"),
            ["技術的意思決定"]               = (3, 3, "インフラ技術選定を根拠を持って説明できる", "技術判断の根拠を丁寧に説明できる"),
            ["金融系業務知識"]               = (2, 2, "業務知識の習得は後回しになっている", "インフラ専門なので許容範囲"),
            ["要件定義・仕様化"]             = (2, 2, "インフラ要件の整理はできる", "アプリ要件への理解を深めてほしい"),
            ["コミュニケーション"]           = (3, 3, "チーム内での技術説明は丁寧にできている", "他部門へのアウトリーチに期待"),
            ["チームワーク・協働"]           = (3, 3, "インフラ担当としてチームをサポートしている", "アプリチームとの協働がさらに必要"),
            ["問題解決力"]                   = (3, 3, "インフラ障害対応で問題解決能力を発揮している", "問題解決力は安定している"),
            ["リーダーシップ・育成"]         = (2, 2, "インフラ分野での育成経験がまだ少ない", "後輩育成への貢献を期待している"),
        });

        AddEvals(talent4.Id, manager2.Id, currPeriod.Id, new()
        {
            ["クラウドアーキテクチャ設計"]  = (1, 1, "まだ学習段階", "今年度は基礎を固めることが優先"),
            ["セキュリティ設計・実装"]       = (2, 2, "セキュリティの基礎知識を習得中", "セキュリティ意識は着実に向上している"),
            ["パフォーマンス最適化"]         = (2, 3, "Core Web Vitalsを意識したUI実装ができるようになった", "FEパフォーマンス意識が芽生えてきた"),
            ["テスト設計・品質保証"]         = (2, 3, "ユニットテストを書く習慣が定着してきた", "テスト品質が向上している"),
            ["アジャイル開発実践"]           = (3, 3, "スクラムへの参加が積極的になった", "アジャイルへの適応が早い"),
            ["コードレビュー"]               = (2, 3, "レビューを受ける中で自分でも指摘できるようになってきた", "成長が見られる"),
            ["技術的意思決定"]               = (2, 2, "技術選択の議論に参加できるようになってきた", "経験を積みながら判断力を育ててほしい"),
            ["金融系業務知識"]               = (1, 2, "業務知識の学習に取り組んでいる", "業務理解の姿勢は良好"),
            ["要件定義・仕様化"]             = (2, 2, "仕様を理解して実装できるようになっている", "要件理解力が伸びている"),
            ["コミュニケーション"]           = (3, 4, "積極的に質問・相談できており、コミュニケーション力は高い", "コミュニケーション能力はチームのプラスになっている"),
            ["チームワーク・協働"]           = (3, 4, "チームワークは非常に良好", "チームへの融合と貢献が増している"),
            ["問題解決力"]                   = (2, 3, "UI改善タスクを自走で解決できた", "問題解決力が着実に向上している"),
            ["リーダーシップ・育成"]         = (1, 2, "まだリーダーシップを発揮する段階ではないが、姿勢は良い", "成長意欲が高く、将来に期待"),
        });

        db.CompetencyEvaluations.AddRange(evals);
        await db.SaveChangesAsync();
    }

    // ── 目標KPI・チェックイン ─────────────────────────────────────────────────
    private static async Task SeedGoalKpisAndCheckinsAsync(AppDbContext db)
    {
        if (db.GoalKpis.Any()) return;

        var talent1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        if (talent1 is null || talent2 is null || talent3 is null) return;

        var currYear   = DateTime.UtcNow.Year;
        var currPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == currYear);
        if (currPeriod is null) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // talent1 の目標KPI
        var g1c_can = db.Goals.FirstOrDefault(g =>
            g.UserId == talent1.Id && g.EvaluationPeriodId == currPeriod.Id &&
            g.Title.Contains("マイクロフロントエンド"));
        var g1c_must = db.Goals.FirstOrDefault(g =>
            g.UserId == talent1.Id && g.EvaluationPeriodId == currPeriod.Id &&
            g.Title.Contains("FEアーキテクチャ"));

        var kpis = new List<GoalKpi>();
        var checkins = new List<GoalCheckin>();

        if (g1c_can is not null)
        {
            var kpi1 = GoalKpi.Create(g1c_can.Id, "PoC完成度", 100, "%",
                "マイクロFE PoC の完成割合を自己評価する", today.AddMonths(3));
            kpis.Add(kpi1);
            checkins.Add(GoalCheckin.Create(g1c_can.Id, kpi1.Id, talent1.Id, 30, 100,
                "モジュールフェデレーションの調査・検証を開始。基本動作は確認できた。"));
            checkins.Add(GoalCheckin.Create(g1c_can.Id, kpi1.Id, talent1.Id, 65, 100,
                "コアモジュールの分割を完了。ルーティング周りの検証が残っている。"));
        }

        if (g1c_must is not null)
        {
            var kpi2 = GoalKpi.Create(g1c_must.Id, "設計書ページ数", 20, "ページ",
                "アーキテクチャ設計書の記述量", today.AddMonths(2));
            kpis.Add(kpi2);
            checkins.Add(GoalCheckin.Create(g1c_must.Id, kpi2.Id, talent1.Id, 8, 20,
                "コンポーネント設計・状態管理の章を完成させた。API設計の章が残っている。"));
            checkins.Add(GoalCheckin.Create(g1c_must.Id, kpi2.Id, talent1.Id, 18, 20,
                "ほぼ完成。部長レビューのフィードバックを反映中。"));
        }

        // talent2 の目標KPI
        var g2c_can = db.Goals.FirstOrDefault(g =>
            g.UserId == talent2.Id && g.EvaluationPeriodId == currPeriod.Id &&
            g.Title.Contains("Service Bus"));
        var g2c_must = db.Goals.FirstOrDefault(g =>
            g.UserId == talent2.Id && g.EvaluationPeriodId == currPeriod.Id &&
            g.Title.Contains("BEアーキテクチャ"));

        if (g2c_can is not null)
        {
            var kpi3 = GoalKpi.Create(g2c_can.Id, "非同期処理スループット", 1000, "msg/sec",
                "Service Bus経由の処理スループット（本番計測）", today.AddMonths(4));
            kpis.Add(kpi3);
            checkins.Add(GoalCheckin.Create(g2c_can.Id, kpi3.Id, talent2.Id, 350, 1000,
                "開発環境でのベンチマーク完了。本番環境での計測準備中。"));
            checkins.Add(GoalCheckin.Create(g2c_can.Id, kpi3.Id, talent2.Id, 820, 1000,
                "本番環境に近い負荷テストで820msg/sec達成。チューニング継続中。"));
        }

        if (g2c_must is not null)
        {
            var kpi4 = GoalKpi.Create(g2c_must.Id, "API実装完了率", 100, "%",
                "計画API数に対する実装・テスト完了済みAPIの割合", today.AddMonths(5));
            kpis.Add(kpi4);
            checkins.Add(GoalCheckin.Create(g2c_must.Id, kpi4.Id, talent2.Id, 40, 100,
                "認証・ユーザー管理APIが完成。評価・スキル管理APIの設計を開始。"));
            checkins.Add(GoalCheckin.Create(g2c_must.Id, kpi4.Id, talent2.Id, 75, 100,
                "主要APIの75%が完成。通知・委員会APIが残っている。"));
        }

        // talent3 の目標KPI
        var g3c_must = db.Goals.FirstOrDefault(g =>
            g.UserId == talent3.Id && g.EvaluationPeriodId == currPeriod.Id &&
            g.Title.Contains("CI/CD"));

        if (g3c_must is not null)
        {
            var kpi5 = GoalKpi.Create(g3c_must.Id, "デプロイ自動化率", 80, "%",
                "全デプロイ操作のうちGitHub Actionsで自動化されている割合", today.AddMonths(6));
            kpis.Add(kpi5);
            checkins.Add(GoalCheckin.Create(g3c_must.Id, kpi5.Id, talent3.Id, 45, 80,
                "フロントエンドとバックエンドの自動デプロイを整備完了。DBマイグレーション自動化が残っている。"));
            checkins.Add(GoalCheckin.Create(g3c_must.Id, kpi5.Id, talent3.Id, 70, 80,
                "DBマイグレーションの自動化も完了。残りは本番環境への適用。"));
        }

        db.GoalKpis.AddRange(kpis);
        await db.SaveChangesAsync();
        db.GoalCheckins.AddRange(checkins);
        await db.SaveChangesAsync();
    }

    // ── フィードバック面談 ────────────────────────────────────────────────────
    private static async Task SeedFeedbackMeetingsAsync(AppDbContext db)
    {
        if (db.FeedbackMeetings.Any()) return;

        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");

        if (talent1 is null || talent2 is null || manager1 is null) return;

        var prevYear   = DateTime.UtcNow.Year - 1;
        var prevPeriod = db.EvaluationPeriods.FirstOrDefault(p => p.Year == prevYear);
        if (prevPeriod is null) return;

        var se1 = db.SelfEvaluations.FirstOrDefault(s => s.UserId == talent1.Id && s.EvaluationPeriodId == prevPeriod.Id);
        var se2 = db.SelfEvaluations.FirstOrDefault(s => s.UserId == talent2.Id && s.EvaluationPeriodId == prevPeriod.Id);

        var meetings = new List<FeedbackMeeting>();

        if (se1 is not null)
        {
            var fm1 = FeedbackMeeting.Create(
                se1.Id, talent1.Id, manager1.Id,
                new DateOnly(prevYear, 12, 20),
                "今年度は全目標を高いレベルで達成した。特にSSRアプリのパフォーマンス改善と社内勉強会3回開催は、チーム全体への貢献として非常に価値が高かった。来年度はテックリードとしてFEアーキテクチャ設計をリードしてもらうことを期待している。",
                "来年度はFEアーキテクチャの設計を一人でリードしてほしい。設計書の品質を高め、チームへの技術展開も継続してほしい。また、社外カンファレンスでの登壇にも挑戦してみてほしい。",
                "今年度の評価に納得している。来年度のテックリードへの期待を聞いて、より具体的なロードマップを話し合いたいと感じた。設計力の向上に意欲的に取り組みたい。");
            fm1.Disclose();
            meetings.Add(fm1);
        }

        if (se2 is not null)
        {
            var fm2 = FeedbackMeeting.Create(
                se2.Id, talent2.Id, manager1.Id,
                new DateOnly(prevYear, 12, 22),
                "今年度最も顕著な成長を見せたエンジニア。クリーンアーキテクチャによるAPI設計・実装と、テストカバレッジ90%以上の達成は、組織全体の技術水準を引き上げる成果だった。",
                "来年度は分散システム・非同期処理の実装経験を積んでほしい。Azure Service Busの設計・実装をメインで担当してもらう予定。また、チームへの技術発信も継続してほしい。",
                "最高評価をいただいて嬉しい。分散システムの実装は自分でも挑戦したいと思っていたので、来年度が楽しみ。設計書作成の習慣を引き続き継続したい。");
            fm2.Disclose();
            meetings.Add(fm2);
        }

        db.FeedbackMeetings.AddRange(meetings);
        await db.SaveChangesAsync();
    }

    // ── クリティカルポジション・サクセッション ──────────────────────────────
    private static async Task SeedCriticalPositionsAsync(AppDbContext db)
    {
        if (db.CriticalPositions.Any()) return;

        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");
        var senior   = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-senior");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null
            || manager1 is null || manager2 is null || senior is null) return;

        // ポジション定義
        var posFL = CriticalPosition.Create(
            "フロントエンドテックリード",
            null,
            PositionImportance.Critical,
            "TypeScript, Vue.js, Nuxt.js Lv4以上。アーキテクチャ設計・チームリードの経験。社内外での技術発信実績。",
            senior.Id);

        var posBE = CriticalPosition.Create(
            "バックエンドアーキテクト",
            null,
            PositionImportance.Critical,
            "C#, ASP.NET Core Lv4以上。クリーンアーキテクチャ・分散システム設計経験。テストカバレッジ管理の実績。",
            senior.Id);

        var posInfra = CriticalPosition.Create(
            "クラウドインフラリード",
            talent3.Id,
            PositionImportance.High,
            "Azure Lv3以上。Docker, GitHub Actions の実務経験。Azure認定資格（Solutions Architect）。",
            senior.Id);

        var posMgr1Backup = CriticalPosition.Create(
            "開発チームマネージャー（山田グループ）後継",
            manager1.Id,
            PositionImportance.High,
            "マネジメント経験。チームメンバーの育成実績。プロジェクト全体の管理経験。",
            senior.Id);

        db.CriticalPositions.AddRange(posFL, posBE, posInfra, posMgr1Backup);
        await db.SaveChangesAsync();

        // 後継者候補
        var sc1 = SuccessionCandidate.Create(
            posFL.Id, talent1.Id,
            ReadinessLevel.ReadySoon,
            "アーキテクチャ設計の実務経験がまだ限定的。設計書の品質向上と社外登壇実績が必要。",
            "新規プロダクトのFEアーキテクチャ設計をリードさせる。社外カンファレンス登壇を今年度中に達成する。",
            senior.Id);

        var sc2 = SuccessionCandidate.Create(
            posFL.Id, talent4.Id,
            ReadinessLevel.ReadyMidTerm,
            "実務経験がまだ浅い。自走での設計・実装実績を積む必要がある。",
            "Nuxt.jsの実務プロジェクト経験を今年度中に最低2件積む。コードレビューで指摘をできるレベルを目指す。",
            senior.Id);

        var sc3 = SuccessionCandidate.Create(
            posBE.Id, talent2.Id,
            ReadinessLevel.ReadySoon,
            "分散システムの実務経験が不足。Azure Service Busの本番運用実績がまだない。",
            "Azure Service Busを使った非同期処理基盤の設計・実装を本番稼働まで担当させる。",
            senior.Id);

        var sc4 = SuccessionCandidate.Create(
            posInfra.Id, talent3.Id,
            ReadinessLevel.ReadyNow,
            "現任者であり後継候補でもある。Azure認定資格取得が完了すれば即戦力。",
            "Azure Solutions Architect Expert の取得を今年度目標とする。",
            senior.Id);

        var sc5 = SuccessionCandidate.Create(
            posMgr1Backup.Id, talent1.Id,
            ReadinessLevel.ReadyMidTerm,
            "テックリード経験を積んでからマネジメントへの移行が望ましい。人事評価・1on1の経験が必要。",
            "まずはテックリードとして実績を作る。3年後をターゲットにマネジメントへの移行を検討する。",
            senior.Id);

        db.SuccessionCandidates.AddRange(sc1, sc2, sc3, sc4, sc5);
        await db.SaveChangesAsync();

        // アクションアイテム
        var actions = new[]
        {
            SuccessionDevelopmentAction.Create(sc1.Id,
                "新規プロダクトのFEアーキテクチャ設計書をQ2末までに完成させ、部長レビューを通過させる",
                new DateOnly(DateTime.UtcNow.Year, 6, 30)),
            SuccessionDevelopmentAction.Create(sc1.Id,
                "社外カンファレンス（Vue Fes Japan または JSConf）への登壇申込を行う",
                new DateOnly(DateTime.UtcNow.Year, 9, 30)),
            SuccessionDevelopmentAction.Create(sc2.Id,
                "Nuxt.jsを使った実務プロジェクトを自走で完遂する（最低2件）",
                new DateOnly(DateTime.UtcNow.Year, 12, 31)),
            SuccessionDevelopmentAction.Create(sc3.Id,
                "Azure Service Busを使った非同期処理基盤の設計書をQ2末までに作成する",
                new DateOnly(DateTime.UtcNow.Year, 6, 30)),
            SuccessionDevelopmentAction.Create(sc3.Id,
                "非同期処理基盤を本番稼働させ、SLO（スループット1000msg/sec）を達成する",
                new DateOnly(DateTime.UtcNow.Year, 9, 30)),
            SuccessionDevelopmentAction.Create(sc4.Id,
                "Azure Solutions Architect Expert 試験を受験・合格する",
                new DateOnly(DateTime.UtcNow.Year, 12, 31)),
            SuccessionDevelopmentAction.Create(sc5.Id,
                "テックリードとして1on1のサポート役を担い、コーチング手法を習得する",
                new DateOnly(DateTime.UtcNow.Year + 1, 3, 31)),
        };

        // 一部のアクションを進行中・完了に更新
        actions[0].UpdateStatus(SuccessionActionStatus.InProgress, null);
        actions[3].UpdateStatus(SuccessionActionStatus.Done, DateTimeOffset.UtcNow.AddDays(-15));

        db.SuccessionDevelopmentActions.AddRange(actions);
        await db.SaveChangesAsync();
    }

    // ── プロジェクトアサイン ──────────────────────────────────────────────────
    private static async Task SeedProjectAssignmentsAsync(AppDbContext db)
    {
        if (db.ProjectAssignments.Any()) return;

        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null
            || manager1 is null || manager2 is null) return;

        var skillTs     = db.SkillItems.FirstOrDefault(s => s.Name == "TypeScript");
        var skillVue    = db.SkillItems.FirstOrDefault(s => s.Name == "Vue.js");
        var skillNuxt   = db.SkillItems.FirstOrDefault(s => s.Name == "Nuxt.js");
        var skillCs     = db.SkillItems.FirstOrDefault(s => s.Name == "C#");
        var skillAspnet = db.SkillItems.FirstOrDefault(s => s.Name == "ASP.NET Core");
        var skillEfcore = db.SkillItems.FirstOrDefault(s => s.Name == "Entity Framework Core");
        var skillAzure  = db.SkillItems.FirstOrDefault(s => s.Name == "Azure");
        var skillDocker = db.SkillItems.FirstOrDefault(s => s.Name == "Docker");
        var skillGha    = db.SkillItems.FirstOrDefault(s => s.Name == "GitHub Actions");

        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var prevYear = DateTime.UtcNow.Year - 1;

        // talent1 のプロジェクト履歴
        var pa1 = ProjectAssignment.Create(talent1.Id,
            "社内ポータルリニューアルプロジェクト",
            "フロントエンドリード",
            new DateOnly(prevYear, 4, 1), new DateOnly(prevYear, 9, 30),
            "社内ポータルをVue.js/Nuxt.jsで全面リニューアル。SSR化によりLCP 4.2秒→1.8秒を達成。");
        pa1.SetManagerComment(
            "SSRアーキテクチャの選定からデプロイまで完全にリードした。Core Web Vitalsの大幅改善は高く評価できる。",
            manager1.Id);

        var pa2 = ProjectAssignment.Create(talent1.Id,
            "タレントマネジメントシステム開発",
            "フロントエンドエンジニア",
            new DateOnly(prevYear, 10, 1), null,
            "社内エンジニア向けタレントマネジメントシステムのFE実装を担当。Nuxt.js + Piniaによるアーキテクチャ設計も主導。");

        // talent2 のプロジェクト履歴
        var pa3 = ProjectAssignment.Create(talent2.Id,
            "社内ポータルリニューアルプロジェクト",
            "バックエンドエンジニア",
            new DateOnly(prevYear, 4, 1), new DateOnly(prevYear, 9, 30),
            "ASP.NET CoreによるREST API設計・実装。クリーンアーキテクチャを採用しテストカバレッジ90%以上を達成。");
        pa3.SetManagerComment(
            "API設計書の品質が格段に向上した。テストカバレッジの高さはチームの資産になっている。",
            manager1.Id);

        var pa4 = ProjectAssignment.Create(talent2.Id,
            "タレントマネジメントシステム開発",
            "バックエンドリード",
            new DateOnly(prevYear, 10, 1), null,
            "バックエンドアーキテクチャの設計からAPI実装・テスト整備まで一人でリード。非同期処理基盤の導入も推進。");

        // talent3 のプロジェクト履歴
        var pa5 = ProjectAssignment.Create(talent3.Id,
            "インフラ刷新プロジェクト",
            "クラウドインフラエンジニア",
            new DateOnly(prevYear, 1, 1), new DateOnly(prevYear, 6, 30),
            "オンプレからAzureへの移行。Container Apps, Azure SQL, Key Vault, Monitor の構成を担当。");
        pa5.SetManagerComment(
            "Azure移行を安定して完遂した。監視・アラートの整備がチームの安心感につながっている。",
            manager2.Id);

        var pa6 = ProjectAssignment.Create(talent3.Id,
            "タレントマネジメントシステム開発",
            "インフラ・CI/CDエンジニア",
            new DateOnly(prevYear, 7, 1), null,
            "Docker化・GitHub Actions CI/CDパイプライン構築・Azure Container Appsへのデプロイ自動化を担当。");

        // talent4 のプロジェクト履歴
        var pa7 = ProjectAssignment.Create(talent4.Id,
            "社内ポータルUI改善プロジェクト",
            "フロントエンドエンジニア",
            new DateOnly(prevYear, 7, 1), new DateOnly(prevYear, 12, 31),
            "既存ポータルのUI改善タスクを担当。Vue.js コンポーネントの改修・アクセシビリティ対応を実施。");
        pa7.SetManagerComment(
            "入社半年でUIコンポーネントを自走で改修できたのは評価できる。来年度はより複雑なタスクに挑戦してほしい。",
            manager2.Id);

        var pa8 = ProjectAssignment.Create(talent4.Id,
            "タレントマネジメントシステム開発",
            "フロントエンドエンジニア",
            new DateOnly(DateTime.UtcNow.Year, 1, 1), null,
            "Nuxt.js によるUIコンポーネント実装を担当。先輩のコードレビューを受けながら品質を高めている。");

        db.ProjectAssignments.AddRange(pa1, pa2, pa3, pa4, pa5, pa6, pa7, pa8);
        await db.SaveChangesAsync();

        // スキルタグ付与
        var paSkills = new List<ProjectAssignmentSkill>();
        if (skillTs is not null && skillVue is not null && skillNuxt is not null)
        {
            paSkills.Add(ProjectAssignmentSkill.Create(pa1.Id, skillTs.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa1.Id, skillVue.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa1.Id, skillNuxt.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa2.Id, skillTs.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa2.Id, skillVue.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa2.Id, skillNuxt.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa7.Id, skillVue.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa7.Id, skillTs.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa8.Id, skillNuxt.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa8.Id, skillVue.Id));
        }
        if (skillCs is not null && skillAspnet is not null && skillEfcore is not null)
        {
            paSkills.Add(ProjectAssignmentSkill.Create(pa3.Id, skillCs.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa3.Id, skillAspnet.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa3.Id, skillEfcore.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa4.Id, skillCs.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa4.Id, skillAspnet.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa4.Id, skillEfcore.Id));
        }
        if (skillAzure is not null && skillDocker is not null && skillGha is not null)
        {
            paSkills.Add(ProjectAssignmentSkill.Create(pa5.Id, skillAzure.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa5.Id, skillDocker.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa6.Id, skillAzure.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa6.Id, skillDocker.Id));
            paSkills.Add(ProjectAssignmentSkill.Create(pa6.Id, skillGha.Id));
        }

        db.ProjectAssignmentSkills.AddRange(paSkills);
        await db.SaveChangesAsync();
    }

    // ── MDC決定・アクションアイテム ──────────────────────────────────────────
    private static async Task SeedMdcDecisionsAsync(AppDbContext db)
    {
        if (db.MdcDecisions.Any()) return;

        var talent1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");
        var senior   = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-senior");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null
            || manager1 is null || manager2 is null || senior is null) return;

        var sessions = db.CommitteeSessions.OrderBy(s => s.HeldOn).ToList();
        if (sessions.Count < 3) return;

        var session1 = sessions[0];
        var session3 = sessions[2];
        var session4 = sessions.Count >= 4 ? sessions[3] : sessions[2];

        var prevYear = DateTime.UtcNow.Year - 1;
        var currYear = DateTime.UtcNow.Year;

        // 第1回セッション決定
        var d1 = MdcDecision.Create(session1.Id, DecisionType.KeyPersonAssign,
            "田中太郎・佐藤花子・伊藤健太を重点管理人物に指定する。各担当マネージャーが育成方針を作成し、次回セッションで報告する。",
            session1.HeldOn, senior.Id);
        d1.Talents.Add(MdcDecisionTalent.Create(d1.Id, talent1.Id));
        d1.Talents.Add(MdcDecisionTalent.Create(d1.Id, talent2.Id));
        d1.Talents.Add(MdcDecisionTalent.Create(d1.Id, talent3.Id));

        // 第3回セッション決定
        var d2 = MdcDecision.Create(session3.Id, DecisionType.KeyPersonRemove,
            "伊藤健太を重点管理人物から解除する。理由：育成目標を達成し自走できると判断したため。",
            session3.HeldOn, senior.Id);
        d2.Talents.Add(MdcDecisionTalent.Create(d2.Id, talent3.Id));

        var d3 = MdcDecision.Create(session3.Id, DecisionType.DevelopmentPolicyChange,
            "田中太郎の来年度育成方針を更新する。テックリードとしてFEアーキテクチャ設計をリードさせる方向で調整する。",
            session3.HeldOn, senior.Id);
        d3.Talents.Add(MdcDecisionTalent.Create(d3.Id, talent1.Id));

        var d4 = MdcDecision.Create(session3.Id, DecisionType.DevelopmentPolicyChange,
            "佐藤花子の来年度育成方針を更新する。分散システム・Azure Service Bus の実装をメインで担当させる。",
            session3.HeldOn, senior.Id);
        d4.Talents.Add(MdcDecisionTalent.Create(d4.Id, talent2.Id));

        // 第4回（今年度第1回）セッション決定
        var d5 = MdcDecision.Create(session4.Id, DecisionType.Other,
            "渡辺美咲のQ3での重点管理人物候補指定を検討する。今年度の業績・成長度合いを評価した上でQ3セッションで判断する。",
            session4.HeldOn, senior.Id);
        d5.Talents.Add(MdcDecisionTalent.Create(d5.Id, talent4.Id));

        db.MdcDecisions.AddRange(d1, d2, d3, d4, d5);
        await db.SaveChangesAsync();

        // アクションアイテム
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var actionItems = new[]
        {
            MdcActionItem.Create(session1.Id, d1.Id,
                "田中太郎の育成方針（初版）を作成し、次回セッション2週間前までに提出する",
                manager1.Id, new DateOnly(prevYear, 8, 31)),
            MdcActionItem.Create(session1.Id, d1.Id,
                "佐藤花子の育成方針（初版）を作成し、次回セッション2週間前までに提出する",
                manager1.Id, new DateOnly(prevYear, 8, 31)),
            MdcActionItem.Create(session1.Id, d1.Id,
                "伊藤健太の育成方針（初版）を作成し、次回セッション2週間前までに提出する",
                manager2.Id, new DateOnly(prevYear, 8, 31)),
            MdcActionItem.Create(session4.Id, d5.Id,
                "渡辺美咲のQ3評価シートを作成し、Q3セッション前に部長へ提出する",
                manager2.Id, today.AddMonths(3)),
            MdcActionItem.Create(session4.Id, null,
                "次回セッション（Q3）のアジェンダを作成する。渡辺美咲の評価・各重点管理人物の中間振り返りを含める",
                senior.Id, today.AddMonths(3)),
        };

        // 前年度分は完了済み
        actionItems[0].UpdateStatus(MdcActionItemStatus.Done);
        actionItems[1].UpdateStatus(MdcActionItemStatus.Done);
        actionItems[2].UpdateStatus(MdcActionItemStatus.Done);
        // 今年度分は進行中
        actionItems[3].UpdateStatus(MdcActionItemStatus.InProgress);

        db.MdcActionItems.AddRange(actionItems);
        await db.SaveChangesAsync();
    }

    // ── 通知 ─────────────────────────────────────────────────────────────────
    private static async Task SeedNotificationsAsync(AppDbContext db)
    {
        if (db.Notifications.Any()) return;

        var talent1  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent1");
        var talent2  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent2");
        var talent3  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent3");
        var talent4  = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-talent4");
        var manager1 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr1");
        var manager2 = db.Users.FirstOrDefault(u => u.EntraObjectId == "dev-mgr2");

        if (talent1 is null || talent2 is null || talent3 is null || talent4 is null
            || manager1 is null || manager2 is null) return;

        var notifications = new List<Notification>();

        // manager1 宛 — 田中・佐藤の自己評価提出通知（未読）
        var n1 = Notification.Create(manager1.Id, NotificationType.SelfEvaluationSubmitted,
            "自己評価が提出されました",
            "田中 太郎 が自己評価を提出しました。上長評価を行ってください。",
            "/periods/current/manager-evaluation");

        var n2 = Notification.Create(manager1.Id, NotificationType.SelfEvaluationSubmitted,
            "自己評価が提出されました",
            "佐藤 花子 が自己評価を提出しました。上長評価を行ってください。",
            "/periods/current/manager-evaluation");

        // talent1 宛 — 考課確定通知（既読）、重点管理候補通知（未読）
        var n3 = Notification.Create(talent1.Id, NotificationType.EvaluationApproved,
            "上長評価が確定されました",
            "山田 一郎 による上長評価が確定されました。フィードバック面談の日程を確認してください。",
            "/periods/current/self-evaluation");
        n3.MarkAsRead();

        var n4 = Notification.Create(talent1.Id, NotificationType.NineBoxKeyPersonCandidate,
            "重点管理候補として評価されました",
            "あなたは重点管理人物候補として評価されています。上長との面談で詳細を確認してください。");

        var n5 = Notification.Create(talent1.Id, NotificationType.KpiCheckinOverdue,
            "KPIチェックインが未実施です",
            "「マイクロフロントエンドアーキテクチャの習得」のKPIチェックインが今月まだ行われていません。",
            "/periods/current/goals");

        // talent2 宛 — 考課確定通知（既読）、後継者候補登録通知（未読）
        var n6 = Notification.Create(talent2.Id, NotificationType.EvaluationApproved,
            "上長評価が確定されました",
            "山田 一郎 による上長評価が確定されました。フィードバック面談の日程を確認してください。",
            "/periods/current/self-evaluation");
        n6.MarkAsRead();

        var n7 = Notification.Create(talent2.Id, NotificationType.SuccessionCandidateAdded,
            "後継者候補として登録されました",
            "あなたは「バックエンドアーキテクト」ポジションの後継者候補として登録されました。",
            "/career");

        var n8 = Notification.Create(talent2.Id, NotificationType.SuccessionGapAlert,
            "スキルギャップアラート",
            "「バックエンドアーキテクト」ポジションに対してスキルギャップがあります。Azure Service Busの実務経験が必要です。",
            "/skills/my");

        // talent3 宛
        var n9 = Notification.Create(talent3.Id, NotificationType.SuccessionCandidateAdded,
            "後継者候補として登録されました",
            "あなたは「クラウドインフラリード」ポジションの後継者候補として登録されました。",
            "/career");

        // talent4 宛 — 考課期間開始通知（既読）、KPIチェックイン督促（未読）
        var n10 = Notification.Create(talent4.Id, NotificationType.EvaluationPeriodStarted,
            "考課期間が開始されました",
            "今年度の考課期間が開始されました。目標設定を行ってください。",
            "/periods/current/goals");
        n10.MarkAsRead();

        var n11 = Notification.Create(talent4.Id, NotificationType.SelfEvaluationDueSoon,
            "自己評価の期限が近づいています",
            "自己評価の提出期限まで3日を切りました。早めに提出してください。",
            "/periods/current/self-evaluation");

        // manager2 宛
        var n12 = Notification.Create(manager2.Id, NotificationType.SelfEvaluationSubmitted,
            "自己評価が提出されました",
            "伊藤 健太 が自己評価を提出しました。上長評価を行ってください。",
            "/periods/current/manager-evaluation");

        notifications.AddRange([n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12]);
        db.Notifications.AddRange(notifications);
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
