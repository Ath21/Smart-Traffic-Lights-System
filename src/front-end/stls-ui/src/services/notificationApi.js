import axios from "axios";

const NOTIF_API = import.meta.env.VITE_NOTIFICATION_API || "http://localhost:5087";

const api = axios.create({
  baseURL: NOTIF_API,
  headers: { "Content-Type": "application/json" }
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem("stls_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// FETCH user notifications
export async function getNotifications(userId, unreadOnly = false) {
  const { data } = await api.get(`/api/notifications/deliveries/${userId}`, {
    params: { unreadOnly }
  });
  return data;
}

// MARK a single delivery as read
export async function markAsRead(userId, email, deliveryId) {
  await api.patch("/api/notifications/deliveries/read", {
    UserId: userId,
    UserEmail: email,
    DeliveryId: deliveryId
  });
}

// GET subscriptions for a user
export async function getSubscriptions(userId) {
  const { data } = await api.get(`/api/notifications/subscriptions/${userId}`);
  return data;
}

// UNSUBSCRIBE
export async function unsubscribe(userId, email, intersection, metric) {
  await api.delete("/api/notifications/subscriptions/unsubscribe", {
    params: {
      UserId: userId,
      UserEmail: email,
      Intersection: intersection,
      Metric: metric
    }
  });
}
