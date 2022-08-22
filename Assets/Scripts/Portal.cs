using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// 
// Teleports to the other portal.
// 

public class Portal : NetworkBehaviour
{
    [SerializeField] public GameObject partner;
    [SerializeField] public float teleDistance;
}