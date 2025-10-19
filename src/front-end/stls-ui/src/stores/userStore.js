import { defineStore } from "pinia";
import {
  loginApi,
  registerApi,
  getProfileApi,
  updateProfileApi,
  resetPasswordApi,
  logoutApi
} from "../services/userApi";

export const useAuth = defineStore("users", {
  state: () => ({
    user: null,
    token: localStorage.getItem("stls_token") || null,
    isLoading: false
  }),

  actions: {
    async login(token, expiresAt) {
      this.token = token;
      localStorage.setItem("stls_token", token);
      localStorage.setItem("stls_token_exp", expiresAt);
      this.user = await getProfileApi();
    },

    async logout() {
      await logoutApi();
      this.user = null;
      this.token = null;
    },

    async fetchProfile() {
      this.user = await getProfileApi();
    }
  }
});
