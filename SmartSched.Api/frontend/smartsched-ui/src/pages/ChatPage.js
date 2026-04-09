import React, { useEffect, useRef, useState } from "react";
import axios from "axios";
import { useAuth } from "../context/AuthContext";

const API_BASE = "https://localhost:7189/api";

function ChatPage() {
    const { user } = useAuth();

    const [contacts, setContacts] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const [messages, setMessages] = useState([]);
    const [messageText, setMessageText] = useState("");

    const token = localStorage.getItem("token");
    const messagesEndRef = useRef(null);

    useEffect(function () {
        loadContacts();
    }, []);

    useEffect(function () {
        let interval;

        if (selectedUser) {
            openConversation(selectedUser);

            interval = setInterval(function () {
                openConversation(selectedUser, false);
            }, 3000);
        } else {
            interval = setInterval(function () {
                loadContacts();
            }, 3000);
        }

        return function () {
            if (interval) clearInterval(interval);
        };
    }, [selectedUser]);

    useEffect(function () {
        scrollToBottom();
    }, [messages]);

    function scrollToBottom() {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: "smooth" });
        }
    }

    async function loadContacts() {
        try {
            const res = await axios.get(`${API_BASE}/chat/contacts`, {
                headers: { Authorization: `Bearer ${token}` }
            });

            setContacts(res.data || []);
        } catch (err) {
            console.error("Failed to load contacts:", err);
        }
    }

    async function openConversation(userToOpen, refreshContacts = true) {
        try {
            const res = await axios.get(`${API_BASE}/chat/conversation/${userToOpen.id}`, {
                headers: { Authorization: `Bearer ${token}` }
            });

            setSelectedUser(userToOpen);
            setMessages(res.data || []);

            if (refreshContacts) {
                await loadContacts();
            }
        } catch (err) {
            console.error("Failed to load conversation:", err);
        }
    }

    async function sendMessage() {
        if (!selectedUser || !messageText.trim()) return;

        try {
            await axios.post(
                `${API_BASE}/chat/send`,
                {
                    receiverId: selectedUser.id,
                    messageText: messageText.trim()
                },
                {
                    headers: { Authorization: `Bearer ${token}` }
                }
            );

            setMessageText("");
            await openConversation(selectedUser);
        } catch (err) {
            console.error("Failed to send message:", err);
            alert(err?.response?.data?.message || "Failed to send message");
        }
    }

    function handleKeyDown(e) {
        if (e.key === "Enter") {
            sendMessage();
        }
    }

    return (
        <div style={{ padding: "20px", height: "calc(100vh - 40px)", boxSizing: "border-box" }}>
            <div
                style={{
                    display: "flex",
                    gap: "20px",
                    height: "100%"
                }}
            >
                <div
                    style={{
                        width: "300px",
                        border: "1px solid #d1d5db",
                        borderRadius: "12px",
                        background: "#ffffff",
                        display: "flex",
                        flexDirection: "column",
                        overflow: "hidden"
                    }}
                >
                    <div
                        style={{
                            padding: "16px",
                            borderBottom: "1px solid #e5e7eb",
                            fontSize: "20px",
                            fontWeight: "600"
                        }}
                    >
                        Chats
                    </div>

                    <div style={{ flex: 1, overflowY: "auto" }}>
                        {contacts.length === 0 && (
                            <div style={{ padding: "16px", color: "#6b7280" }}>
                                No available contacts.
                            </div>
                        )}

                        {contacts.map(function (contact) {
                            const isSelected = selectedUser && selectedUser.id === contact.id;

                            return (
                                <div
                                    key={contact.id}
                                    onClick={() => openConversation(contact)}
                                    style={{
                                        padding: "14px 16px",
                                        borderBottom: "1px solid #f3f4f6",
                                        cursor: "pointer",
                                        background: isSelected ? "#eff6ff" : "#ffffff",
                                        display: "flex",
                                        justifyContent: "space-between",
                                        alignItems: "center"
                                    }}
                                >
                                    <div>
                                        <div style={{ fontWeight: "600", color: "#111827" }}>
                                            {contact.fullName}
                                        </div>
                                        <div style={{ fontSize: "12px", color: "#6b7280", marginTop: "2px" }}>
                                            {contact.email}
                                        </div>
                                    </div>

                                    {contact.unreadCount > 0 && (
                                        <div
                                            style={{
                                                minWidth: "22px",
                                                height: "22px",
                                                borderRadius: "999px",
                                                background: "#2563eb",
                                                color: "#ffffff",
                                                fontSize: "12px",
                                                display: "flex",
                                                alignItems: "center",
                                                justifyContent: "center",
                                                padding: "0 6px",
                                                fontWeight: "600"
                                            }}
                                        >
                                            {contact.unreadCount}
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </div>

                <div
                    style={{
                        flex: 1,
                        border: "1px solid #d1d5db",
                        borderRadius: "12px",
                        background: "#ffffff",
                        display: "flex",
                        flexDirection: "column",
                        overflow: "hidden"
                    }}
                >
                    <div
                        style={{
                            padding: "16px",
                            borderBottom: "1px solid #e5e7eb",
                            fontSize: "20px",
                            fontWeight: "600"
                        }}
                    >
                        {selectedUser ? selectedUser.fullName : "Select a chat"}
                    </div>

                    <div
                        style={{
                            flex: 1,
                            overflowY: "auto",
                            padding: "20px",
                            background: "#f9fafb"
                        }}
                    >
                        {!selectedUser && (
                            <div style={{ color: "#6b7280" }}>Choose a contact from the left.</div>
                        )}

                        {selectedUser && messages.length === 0 && (
                            <div style={{ color: "#6b7280" }}>No messages yet.</div>
                        )}

                        {selectedUser &&
                            messages.map(function (m) {
                                const isMine =
                                    String(m.senderId).trim() === String(user?.id).trim();

                                return (
                                    <div
                                        key={m.id}
                                        style={{
                                            display: "flex",
                                            justifyContent: isMine ? "flex-end" : "flex-start",
                                            marginBottom: "12px"
                                        }}
                                    >
                                        <div
                                            style={{
                                                maxWidth: "60%",
                                                padding: "10px 14px",
                                                borderRadius: "16px",
                                                background: isMine ? "#2563eb" : "#e5e7eb",
                                                color: isMine ? "#ffffff" : "#111827",
                                                boxShadow: "0 1px 2px rgba(0,0,0,0.08)"
                                            }}
                                        >
                                            <div style={{ fontSize: "14px", wordBreak: "break-word" }}>
                                                {m.messageText}
                                            </div>
                                            <div
                                                style={{
                                                    fontSize: "11px",
                                                    opacity: 0.8,
                                                    marginTop: "4px",
                                                    textAlign: isMine ? "right" : "left"
                                                }}
                                            >
                                                {new Date(m.sentAt).toLocaleString()}
                                            </div>
                                        </div>
                                    </div>
                                );
                            })}

                        <div ref={messagesEndRef}></div>
                    </div>

                    {selectedUser && (
                        <div
                            style={{
                                borderTop: "1px solid #e5e7eb",
                                padding: "16px",
                                display: "flex",
                                gap: "12px",
                                background: "#ffffff"
                            }}
                        >
                            <input
                                type="text"
                                value={messageText}
                                onChange={(e) => setMessageText(e.target.value)}
                                onKeyDown={handleKeyDown}
                                placeholder="Type a message..."
                                style={{
                                    flex: 1,
                                    padding: "12px 14px",
                                    borderRadius: "10px",
                                    border: "1px solid #d1d5db",
                                    outline: "none",
                                    fontSize: "14px"
                                }}
                            />
                            <button
                                onClick={sendMessage}
                                style={{
                                    padding: "12px 18px",
                                    borderRadius: "10px",
                                    border: "none",
                                    background: "#2563eb",
                                    color: "#ffffff",
                                    fontWeight: "600",
                                    cursor: "pointer"
                                }}
                            >
                                Send
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default ChatPage;