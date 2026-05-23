using Microsoft.AspNetCore.Mvc;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
public class MeetingTemplatesController : ControllerBase
{
    // GET /api/one-on-one-templates
    [HttpGet("api/one-on-one-templates")]
    public IActionResult GetTemplates()
    {
        var templates = new[]
        {
            new
            {
                id = "will-can-must",
                name = "Will-Can-Must振り返り",
                willPlaceholder = "やりたいこと・目指したいキャリア・成長への意欲",
                canPlaceholder = "現在できること・強み・スキル",
                mustPlaceholder = "組織・役割から求められること・やるべきこと",
                freeTextPlaceholder = (string?)null
            },
            new
            {
                id = "skill-development",
                name = "スキル開発",
                willPlaceholder = "習得したい技術・スキル",
                canPlaceholder = "現在のスキルレベル・直近の学習状況",
                mustPlaceholder = "業務で必要なスキル・チームへの貢献",
                freeTextPlaceholder = "学習計画・リソース・サポートしてほしいこと"
            },
            new
            {
                id = "career-plan",
                name = "キャリアプラン",
                willPlaceholder = "3〜5年後のキャリアイメージ・なりたい姿",
                canPlaceholder = "現在の強み・経験・実績",
                mustPlaceholder = "目標達成のために取り組むべきこと",
                freeTextPlaceholder = "組織への要望・サポート・懸念点"
            },
        };

        return Ok(templates);
    }
}
