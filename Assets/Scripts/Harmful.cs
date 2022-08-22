using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Harmful : NetworkBehaviour
{
    [SerializeField] public float damage;
}