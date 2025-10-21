import { defineStore } from "pinia";
import { getSubscriptions, unsubscribe } from "../services/notificationApi";
import { decodeToken } from "../utils/jwtHelper";

export const useNotifications = defineStore("notifications", {
  state: () => ({
    subscriptions: [],
    notifications: [],
    unreadCount: 0,
    isLoading: false,
    error: null
  }),

  actions: {
    getUserId() {
      const token = localStorage.getItem("stls_token");
      const decoded = decodeToken(token);
      return decoded?.id || null;
    },

    getUserEmail() {
      const token = localStorage.getItem("stls_token");
      const decoded = decodeToken(token);
      return decoded?.email || null;
    },

    async fetchSubscriptions() {
      const userId = this.getUserId();
      if (!userId) {
        this.error = "User not authenticated";
        return;
      }

      this.isLoading = true;
      try {
        this.subscriptions = await getSubscriptions(userId);
        this.error = null;
      } catch (err) {
        console.error(err);
        this.error = err.message || "Failed to fetch subscriptions";
      } finally {
        this.isLoading = false;
      }
    },

    async removeSubscription(intersection, metric) {
      const userId = this.getUserId();
      const email = this.getUserEmail();
      if (!userId || !email) {
        this.error = "User not authenticated";
        return;
      }

      try {
        await unsubscribe(userId, email, intersection, metric);
        this.subscriptions = this.subscriptions.filter(
          s => !(s.Intersection === intersection && s.Metric === metric)
        );
      } catch (err) {
        console.error(err);
        this.error = err.message || "Failed to unsubscribe";
      }
    }
  }
});
