import requests

from functools import wraps
from telegram import Update, ChatPermissions
from telegram.ext import CallbackContext
from datetime import datetime, timedelta
from time import sleep

from src.const import API_BASE_URL
from src.helpers.user_roles import UserRoles


def auth(func):
    """
    Register the User if his not already registered.
    """

    @wraps(func)
    def wrapper(this, update: Update, context: CallbackContext):
        user = requests.get(f"{API_BASE_URL}/api/user", params={
            "telegramId": update.message.from_user.id,
        })

        if user.status_code == 404:
            # Create new User.
            user = requests.put(
                f"{API_BASE_URL}/api/user/{update.message.from_user.id}",
                headers={
                    "Content-Type": "application/json",
                }
            )

        user = user.json()

        # Deny access for the banned users.
        if UserRoles.Banned in UserRoles(user["roles"]):
            update.get_bot().restrict_chat_member(
                chat_id=update.message.chat.id,
                user_id=update.message.from_user.id,
                permissions=ChatPermissions.no_permissions(),
                until_date=datetime.now() + timedelta(days=3650),
            )

            return

        return func(this, update, context)

    return wrapper
