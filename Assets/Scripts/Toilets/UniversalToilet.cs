using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalToilet : Toilet
{
    public override bool GenderIsFit(Unit character)
    {
        return true;
    }
}
