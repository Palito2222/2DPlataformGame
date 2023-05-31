using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemiesData", menuName = "Custom/EnemiesData")]
public class WorldEnemieData : ScriptableObject
{
    [BoxGroup("Información Básica")]
    [LabelWidth(120)]
    public string enemieName;

    [BoxGroup("Información Básica")]
    [LabelWidth(100)]
    [TextArea]
    public string enemieDescription;

    [BoxGroup("Información Básica")]
    [LabelWidth(120)]
    public string uniqueID;

    [Title("Sprite", bold: true, horizontalLine: true, TitleAlignment = TitleAlignments.Left)]

    [PreviewField(75, Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Left)]
    [HideLabel]
    public Sprite characterSprite;

    [Title("Movimiento", bold: true, horizontalLine: true, TitleAlignment = TitleAlignments.Left)]

    [VerticalGroup("Game Data")]
    [GUIColor("#8EF8FF")]
    public float moveSpeed;

    [VerticalGroup("Game Data")]
    [GUIColor("#8EF8FF")]
    public float jumpSpeed;

    [VerticalGroup("Game Data")]
    [GUIColor("#8EF8FF")]
    public float maxSlopeAngle;

    [Title("Estadísticas", bold: true, horizontalLine: true, TitleAlignment = TitleAlignments.Left)]

    [VerticalGroup("Stats")]
    [GUIColor("#BBFE6F")]
    public float health;

    [VerticalGroup("Stats")]
    [GUIColor("#FE6F6F")]
    public float attack;

    [VerticalGroup("Stats")]
    [GUIColor("#FE6F3F")]
    public float attackRadius;

    [VerticalGroup("Stats")]
    [GUIColor("#FEFE6F")]
    public float defense;

    [VerticalGroup("Stats")]
    [GUIColor("#B595FF")]
    public float probCrit;

    [VerticalGroup("Stats")]
    [GUIColor("#EA95FF")]
    public float dmgCrit;
}
