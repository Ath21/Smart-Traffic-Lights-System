import { defineStore } from "pinia";
import { getNotifications, markAsRead } from "../services/notificationApi";
import { useAuth } from "./userStore";

export const useNotifications = defineStore("notifications", {
  state: () => ({
    notifications: [],
    isLoading: false,
    error: null,
    unreadCount: 0,
    intervalId: null
  }),

  actions: {
    async fetchNotifications() {
      const auth = useAuth();
      if (!auth.user) return;
      this.isLoading = true;
      try {
        const data = await getNotifications(auth.user.userId);
        this.notifications = data;
        this.unreadCount = data.filter(n => !n.readAt).length;
        this.error = null;
      } catch (err) {
        this.error = err.message;
      } finally {
        this.isLoading = false;
      }
    },

    async markAsRead(deliveryId) {
      const auth = useAuth();
      await markAsRead(auth.user.userId, auth.user.email, deliveryId);
      this.notifications = this.notifications.map(n =>
        n.deliveryId === deliveryId ? { ...n, readAt: new Date().toISOString() } : n
      );
      this.unreadCount = this.notifications.filter(n => !n.readAt).length;
    },

    startPolling(ms = 10000) {
      this.stopPolling();
      this.fetchNotifications();
      this.intervalId = setInterval(this.fetchNotifications, ms);
    },

    stopPolling() {
      if (this.intervalId) clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }
});
