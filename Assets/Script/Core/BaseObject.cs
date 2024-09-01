using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : MonoBehaviour
{
    GameObject cacheObject = null;
    Transform cacheTransform = null;
    Rigidbody2D cacheRigidbody = null;
    Collider2D cacheCollider = null;

    public GameObject GAMEOBJECT
    {
        get
        {
            if(!cacheObject || cacheObject == null)
            {
                cacheObject = gameObject;
            }

            return cacheObject;
        }
    }

    public Transform TRANSFORM
    { 
        get
        {
            if (cacheTransform == null)
            {
                cacheTransform = GetComponent<Transform>();
            }

            return cacheTransform;
        }
    }

    public Rigidbody2D RIGIDBODY
    {
        get
        {
            if (cacheRigidbody == null && TryGetComponent<Rigidbody2D>(out var comp))
            {
                cacheRigidbody = comp;
            }

            return cacheRigidbody;
        }
    }

    public Collider2D COLLIDER
    {
        get
        {
            if (cacheCollider == null && TryGetComponent<Collider2D>(out var comp))
            {
                cacheCollider = comp;
            }

            return cacheCollider;
        }
    }
}
