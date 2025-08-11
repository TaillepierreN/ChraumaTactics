using UnityEngine;

public class Snipers : Unit
{
	// Laser scope on target
	// Potential Skill options for Snipers
	// 1. Range Enhancement: Increases the range of Snipers' attacks.
	// 2. Evolution: The effect increases with unit level, increasing ATK by 20% and Range by 5% per level
	// 3. Assault Mode: Snipers range is reduced by 70%, HP is increased by 500%, movement speed is increased by 3, and attack becomes AOE.

	protected override void Start()
	{
		base.Start();
		animatorBody.SetBool("IsSnipe", true);
	}
}
