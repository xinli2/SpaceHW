using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.ResourceSystem
{
    /// <summary>
    /// A resource type.
    /// </summary>
    [CreateAssetMenu(menuName = "VSX/Resource Type")]
    public class ResourceType : ScriptableObject
    {

        [Tooltip("The long (full) name of the resource type.")]
        [SerializeField]
        protected string shortName;
        public virtual string ShortName
        {
            get { return shortName; }
            set { shortName = value; }
        }


        [Tooltip("The short (abbreviated) name of the resource type.")]
        [SerializeField]
        protected string longName;
        public virtual string LongName
        {
            get { return longName; }
            set { longName = value; }
        }


        [Tooltip("The icon for this resource type.")]
        [SerializeField]
        protected Sprite icon;
        public virtual Sprite Icon
        {
            get { return icon; }
            set { icon = value; }
        }
    }
}
