﻿using DA_Assets.FCU.Model;
using DA_Assets.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class TagExtensions
    {
        public static bool HaveUndownloadableTags(this FObject fobject, out string reason)
        {
            bool result = false;
            reason = null;

            if (fobject.ContainsTag(FcuTag.Image) == false)
            {
                reason = "fobject.ContainsTag(FcuTag.Image) == false)";
                result = true;
            }
            else
            {
                foreach (FcuTag fcuTag in fobject.Data.Tags)
                {
                    TagConfig tc = fcuTag.GetTagConfig();

                    if (tc.CanBeDownloaded == false)
                    {
                        reason = $"{fobject.Data.Tags.ToLine()}";
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public static void RemoveNotDownloadableTags(this FObject fobject)
        {
            List<FcuTag> newTags = new List<FcuTag>();

            foreach (FcuTag tag in fobject.Data.Tags)
            {
                TagConfig tc = tag.GetTagConfig();

                if (tc.CanBeDownloaded)
                {
                    newTags.Add(tag);
                }
            }

            fobject.Data.Tags = newTags;
        }

        public static void AddTag(this FObject fobject, FcuTag tag)
        {
            //Debug.Log($"AddTag | {fobject.Data.NameHierarchy} | {tag}");

            if (fobject.Data.Tags == null)
                fobject.Data.Tags = new List<FcuTag>();

            if (fobject.Data.Tags.Contains(tag) == false)
                fobject.Data.Tags.Add(tag);
        }

        public static bool ContainsCustomButtonTags(this SyncHelper syncHelper) =>
            syncHelper.ContainsAnyTag(
                FcuTag.BtnDefault,
                FcuTag.BtnDisabled,
                FcuTag.BtnHover,
                FcuTag.BtnPressed,
                FcuTag.BtnSelected);

        public static bool ContainsCustomButtonTags(this FObject fobject) => 
            fobject.ContainsAnyTag(
                FcuTag.BtnDefault,
                FcuTag.BtnDisabled,
                FcuTag.BtnHover,
                FcuTag.BtnPressed,
                FcuTag.BtnSelected);

        public static bool IsRootSprite(this FObject child, FObject parent)
        {
            bool value = false;

            if (parent.ContainsTag(FcuTag.Frame))
            {
                switch (child.Type)
                {
                    case NodeType.VECTOR:
                    case NodeType.BOOLEAN_OPERATION:
                    case NodeType.STAR:
                    case NodeType.REGULAR_POLYGON:
                        {
                            value = true;
                        }
                        break;
                }
            }

            return value;
        }

        public static bool ContainsAnyTag(this FObject fobject, params FcuTag[] tags)
        {
            if (fobject.Data == null)
            {
                Debug.LogWarning("fobject.Data == null");
                return false;
            }

            if (fobject.Data.Tags.IsEmpty())
                return false;

            foreach (FcuTag tag in tags)
            {
                if (fobject.ContainsTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAnyTag(this SyncHelper syncHelper, params FcuTag[] tags)
        {
            if (syncHelper?.Data == null)
            {
                Debug.LogWarning("syncHelper.Data == null");
                return false;
            }

            if (syncHelper.Data.Tags.IsEmpty())
                return false;

            foreach (FcuTag tag in tags)
            {
                if (syncHelper.ContainsTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsTag(this SyncHelper syncHelper, FcuTag tag)
        {
            if (syncHelper?.Data == null)
            {
                //Debug.LogWarning("syncHelper.Data == null");
                return false;
            }

            if (syncHelper.Data.Tags.IsEmpty())
                return false;

            return syncHelper.Data.Tags.Contains(tag);
        }

        public static bool ContainsTag(this FObject fobject, FcuTag tag)
        {
            if (fobject.Data == null)
            {
                //Debug.LogWarning("fobject.Data == null");
                return false;
            }

            if (fobject.Data.Tags.IsEmpty())
                return false;

            return fobject.Data.Tags.Contains(tag);
        }

        public static TagConfig GetTagConfig(this FcuTag fcuTag)
        {
            TagConfig tagConfig = FcuConfig.TagConfigs.FirstOrDefault(x => x.FcuTag == fcuTag);

            if (tagConfig.IsDefault())
            {
                Debug.LogError($"No tag config for '{fcuTag}' tag.");
                return new TagConfig();
            }

            return tagConfig;
        }

        public static string ToLine(this IList<FcuTag> tags)
        {
            return string.Join(", ", tags);
        }
    }
}