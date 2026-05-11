using UnityEngine;
using System.Collections;

public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    public string baseSpellId;
    public string modifierSpellId;
    public SpellData baseSpellData;
    public SpellData modifierSpellData;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public Spell(SpellCaster owner, string baseSpellId, SpellData baseSpellData, string modifierSpellId, SpellData modifierSpellData)
    {
        this.owner = owner;
        this.baseSpellId = baseSpellId;
        this.baseSpellData = baseSpellData;
        this.modifierSpellId = modifierSpellId;
        this.modifierSpellData = modifierSpellData;
    }

    public string GetName()
    {
        if (baseSpellData == null)
        {
            return "Bolt";
        }

        if (modifierSpellData == null)
        {
            return baseSpellData.name;
        }

        return modifierSpellData.name + " " + baseSpellData.name;
    }

    public string GetDescription()
    {
        if (baseSpellData == null)
        {
            return "";
        }

        if (modifierSpellData == null)
        {
            return baseSpellData.description;
        }

        return baseSpellData.description + "\n" + modifierSpellData.description;
    }

    public int GetManaCost()
    {
        if (baseSpellData == null)
        {
            return 10;
        }

        return 10;
    }

    public int GetDamage()
    {
        return 100;
    }

    public float GetCooldown()
    {
        return 0.75f;
    }

    public virtual int GetIcon()
    {
        if (baseSpellData == null)
        {
            return 0;
        }

        return baseSpellData.icon;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

    }

}
