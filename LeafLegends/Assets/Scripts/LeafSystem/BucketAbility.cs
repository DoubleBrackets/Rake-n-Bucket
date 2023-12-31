using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.Events;

public class BucketAbility : MonoBehaviour
{
    public bool CanBucket { get; set; } = true;

    public UnityEvent OnCollect;

    private void OnTriggerStay2D(Collider2D other)
    {
        TryBucket(other);
    }

    private void TryBucket(Collider2D coll)
    {
        if (!CanBucket)
        {
            return;
        }

        var leafInstance = coll.GetComponent<LeafInstance>();
        if (leafInstance)
        {
            if (leafInstance.TryCollectWithBucket())
            {
                AudioManager.Instance.PlaySFX(SFX.LeafCollected, transform.position);
                OnCollect?.Invoke();
            }
        }
    }
}