import { hasPerms } from "@/utils/auth";
import type { Directive, DirectiveBinding } from "vue";

export const permission: Directive = {
  mounted(el: HTMLElement, binding: DirectiveBinding<string | Array<string>>) {
    const { value } = binding;
    if (value) {
      !hasPerms(value) && el.parentNode?.removeChild(el);
    } else {
      throw new Error(
        "[Directive: permission]: need permissions! Like v-permission=\"['btn.add','btn.edit']\""
      );
    }
  }
};
