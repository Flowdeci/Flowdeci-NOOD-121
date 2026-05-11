using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
        return GetManaCost(GetOwnerSpellPower());
    }

    public int GetDamage()
    {
        return GetDamage(GetOwnerSpellPower());
    }

    public float GetCooldown()
    {
        return GetCooldown(GetOwnerSpellPower());
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

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, int spellPower)
    {
        this.team = team;
        last_cast = Time.time;

        if (modifierSpellData != null && !string.IsNullOrEmpty(modifierSpellData.angle))
        {
            float angle = EvaluateFormula(modifierSpellData.angle, spellPower, 0);
            FireProjectile(where, target, spellPower, -angle);
            FireProjectile(where, target, spellPower, angle);
            yield return new WaitForEndOfFrame();
            yield break;
        }

        FireProjectile(where, target, spellPower, 0);

        if (modifierSpellData != null && !string.IsNullOrEmpty(modifierSpellData.delay))
        {
            float delay = EvaluateFormula(modifierSpellData.delay, spellPower, 0);
            yield return new WaitForSeconds(delay);
            FireProjectile(where, target, spellPower, 0);
        }

        yield return new WaitForEndOfFrame();
    }

    private void FireProjectile(Vector3 where, Vector3 target, int spellPower, float angleOffset)
    {
        Vector3 direction = target - where;
        if (direction == Vector3.zero)
        {
            direction = Vector3.right;
        }

        direction = Quaternion.Euler(0, 0, angleOffset) * direction;

        int damage = GetDamage(spellPower);
        Damage.Type damageType = GetDamageType();
        Action<Hittable, Vector3> onHit = MakeOnHit(damage, damageType, team);

        int sprite = GetProjectileSprite();
        string trajectory = GetProjectileTrajectory();
        float speed = GetProjectileSpeed(spellPower);
        float lifetime = GetProjectileLifetime(spellPower);

        if (lifetime > 0)
        {
            GameManager.Instance.projectileManager.CreateProjectile(sprite, trajectory, where, direction, speed, onHit, lifetime);
        }
        else
        {
            GameManager.Instance.projectileManager.CreateProjectile(sprite, trajectory, where, direction, speed, onHit);
        }
    }

    private Action<Hittable, Vector3> MakeOnHit(int damage, Damage.Type damageType, Hittable.Team castTeam)
    {
        return (Hittable other, Vector3 impact) =>
        {
            if (other.team != castTeam)
            {
                other.Damage(new Damage(damage, damageType));
            }
        };
    }

    private int GetManaCost(int spellPower)
    {
        if (baseSpellData == null)
        {
            return 10;
        }

        float manaCost = EvaluateFormula(baseSpellData.mana_cost, spellPower, 10);
        manaCost *= EvaluateFormula(GetModifierValue("mana_multiplier"), spellPower, 1);
        manaCost += EvaluateFormula(GetModifierValue("mana_adder"), spellPower, 0);

        return Mathf.Max(0, Mathf.RoundToInt(manaCost));
    }

    private int GetDamage(int spellPower)
    {
        if (baseSpellData == null || baseSpellData.damage == null)
        {
            return 100;
        }

        float damage = EvaluateFormula(baseSpellData.damage.amount, spellPower, 100);
        damage *= EvaluateFormula(GetModifierValue("damage_multiplier"), spellPower, 1);

        return Mathf.Max(0, Mathf.RoundToInt(damage));
    }

    private float GetCooldown(int spellPower)
    {
        if (baseSpellData == null)
        {
            return 0.75f;
        }

        float cooldown = EvaluateFormula(baseSpellData.cooldown, spellPower, 0.75f);
        cooldown *= EvaluateFormula(GetModifierValue("cooldown_multiplier"), spellPower, 1);

        return Mathf.Max(0, cooldown);
    }

    private float GetProjectileSpeed(int spellPower)
    {
        if (baseSpellData == null || baseSpellData.projectile == null)
        {
            return 15;
        }

        float speed = EvaluateFormula(baseSpellData.projectile.speed, spellPower, 15);
        speed *= EvaluateFormula(GetModifierValue("speed_multiplier"), spellPower, 1);

        return speed;
    }

    private float GetProjectileLifetime(int spellPower)
    {
        if (baseSpellData == null || baseSpellData.projectile == null || string.IsNullOrEmpty(baseSpellData.projectile.lifetime))
        {
            return -1;
        }

        return EvaluateFormula(baseSpellData.projectile.lifetime, spellPower, -1);
    }

    private int GetProjectileSprite()
    {
        if (baseSpellData == null || baseSpellData.projectile == null)
        {
            return 0;
        }

        return baseSpellData.projectile.sprite;
    }

    private string GetProjectileTrajectory()
    {
        string modifierTrajectory = GetModifierValue("projectile_trajectory");
        if (!string.IsNullOrEmpty(modifierTrajectory))
        {
            return modifierTrajectory;
        }

        if (baseSpellData == null || baseSpellData.projectile == null)
        {
            return "straight";
        }

        return baseSpellData.projectile.trajectory;
    }

    private Damage.Type GetDamageType()
    {
        if (baseSpellData == null || baseSpellData.damage == null)
        {
            return Damage.Type.ARCANE;
        }

        return Damage.TypeFromString(baseSpellData.damage.type);
    }

    private string GetModifierValue(string field)
    {
        if (modifierSpellData == null)
        {
            return null;
        }

        if (field == "damage_multiplier") return modifierSpellData.damage_multiplier;
        if (field == "mana_multiplier") return modifierSpellData.mana_multiplier;
        if (field == "speed_multiplier") return modifierSpellData.speed_multiplier;
        if (field == "cooldown_multiplier") return modifierSpellData.cooldown_multiplier;
        if (field == "projectile_trajectory") return modifierSpellData.projectile_trajectory;
        if (field == "mana_adder") return modifierSpellData.mana_adder;

        return null;
    }

    private float EvaluateFormula(string formula, int spellPower, float fallback)
    {
        if (string.IsNullOrEmpty(formula))
        {
            return fallback;
        }

        Dictionary<string, int> variables = new Dictionary<string, int>();
        variables["power"] = spellPower;
        variables["wave"] = GameManager.Instance.currentWave;

        try
        {
            return RPNEvaluator.RPNEvaluator.Evaluatef(formula, variables);
        }
        catch (Exception error)
        {
            Debug.LogError("Could not evaluate spell formula '" + formula + "': " + error.Message);
            return fallback;
        }
    }

    private int GetOwnerSpellPower()
    {
        if (owner == null)
        {
            return 0;
        }

        return owner.spell_power;
    }

}
