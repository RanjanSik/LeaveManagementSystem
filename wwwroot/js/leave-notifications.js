(function () {
  if (typeof signalR === "undefined") {
    return;
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/leaveNotifications")
    .withAutomaticReconnect()
    .build();

  connection.on("LeaveStatusUpdated", function (payload) {
    const toastEl = document.getElementById("notificationToast");
    const bodyEl = document.getElementById("notificationToastBody");
    if (!toastEl || !bodyEl) {
      return;
    }

    bodyEl.textContent = payload.message || "Your leave request status was updated.";
    const toast = bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 8000 });
    toast.show();
  });

  connection.start().catch(function (err) {
    console.warn("SignalR connection failed:", err);
  });
})();
