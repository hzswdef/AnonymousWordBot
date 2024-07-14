import logging
import re
from typing import BinaryIO

import requests

from re import match
from redis import Redis
from telegram import Update, User, InputMediaPhoto
from telegram.constants import ParseMode
from telegram.error import BadRequest
from telegram.helpers import escape_markdown
from telegram.ext import (
    Application,
    CommandHandler,
    MessageHandler,
    CallbackContext,
    filters,
)

from src.const import API_BASE_URL, USER_LINK_REGEX, USER_WELCOME_REGEX
from src.decorators.auth import auth
from src.decorators.admin import admin
from src.markup import Markup
from src.helpers.reply_templates import load_reply_templates
from src.helpers.user_roles import UserRoles
from src.utilities.env import load_env


class TelegramBot:

    def __init__(self):
        # Set up logger.
        logging.basicConfig(
            format='[%(asctime)s] %(message)s',
            level=logging.INFO,
        )
        self.logger = logging.getLogger('bot')

        self.redis = Redis(decode_responses=True)
        self.redis.ping()

        self.env = load_env()
        self.replies = load_reply_templates()

        # self.input_handler = InputHandler(self.logger, self.database_handler)

        self.markup = Markup()

        self.bot = Application.builder().token(self.env.TELEGRAM_BOT_TOKEN).build()

        # Register command handlers.
        self.bot.add_handler(CommandHandler('start', self.start_command))
        # @TODO Implement this.
        # self.bot.add_handler(CommandHandler('donate', self.donate_command))
        self.bot.add_handler(CommandHandler('link', self.link_command))
        self.bot.add_handler(CommandHandler('welcome', self.welcome_command))
        self.bot.add_handler(CommandHandler('delete', self.delete_command))
        # self.bot.add_handler(CommandHandler('stats', self.stats_command))
        # self.bot.add_handler(CommandHandler('help', self.help_command))
        self.bot.add_handler(CommandHandler('reveal', self.reveal_command))

        # Register message handler.
        self.bot.add_handler(MessageHandler(filters.ALL, self.handle_message))

        self.logger.info('Initialized the Telegram bot.')

    @auth
    async def start_command(self, update: Update, context: CallbackContext):
        """ Start command. """

        receiver_link = None

        author = requests.get(f"{API_BASE_URL}/api/user", params={
            "telegramId": update.message.from_user.id,
        })

        if author.status_code != 200:
            return await self.handle_error(
                update,
                context,
                f"FATAL: GET /api/user [tid: {update.message.from_user.id}] ({author.status_code}) - User not registered?"
            )

        if len(context.args) != 0:
            receiver_link = context.args[0]

        author = author.json()

        # Send the usual "start" command message if no receiver link is present.
        if receiver_link is None:
            if author["link"] is None:
                return await update.message.reply_text(
                    text=self.replies.COMMAND_START["NO_LINK"],
                    parse_mode=ParseMode.MARKDOWN,
                )

            return await update.message.reply_text(
                text=self.replies.COMMAND_START["DEFAULT"].format(
                    bot_username=escape_markdown(update.get_bot().username),
                    user_link=escape_markdown(author["link"]),
                ),
                parse_mode=ParseMode.MARKDOWN,
            )

        receiver = requests.get(f"{API_BASE_URL}/api/user", params={
            "link": receiver_link,
        })

        if receiver.status_code != 200:
            return await update.message.reply_text("Похоже получатель изменил или удалил ссылку...")

        receiver = receiver.json()

        session = self.redis.set(
            f"session:{update.message.from_user.id}:message",
            receiver_link,
            1800,
        )

        await update.message.reply_text(
            text=self.replies.COMMAND_START["ANONYMOUS_MESSAGE"],
            parse_mode=ParseMode.MARKDOWN,
        )

        if receiver["welcomeMessage"]:
            await update.message.reply_text(
                text=receiver["welcomeMessage"],
                parse_mode=ParseMode.MARKDOWN,
            )

    # @TODO Implement this.
    # @auth
    # async def donate_command(self, update: Update, context: CallbackContext):
    #     """ Donate command. """
    #
    #     await update.message.reply_text(text=self.replies.COMMAND_DONATE, parse_mode=ParseMode.MARKDOWN)

    @auth
    async def link_command(self, update: Update, context: CallbackContext):
        """ Command to change the User's link. """

        link = "".join(context.args)

        if link == "":
            user = requests.get(f"{API_BASE_URL}/api/user", params={
                "telegramId": update.message.from_user.id,
            })
            user = user.json()

            if user["link"] is None:
                return await update.message.reply_text(
                    text=self.replies.COMMAND_LINK["NO_LINK"],
                    parse_mode=ParseMode.MARKDOWN,
                )

            return await update.message.reply_text(
                text=self.replies.COMMAND_LINK["DEFAULT"].format(
                    bot_username=update.get_bot().username,
                    link=user["link"],
                ),
                parse_mode=ParseMode.MARKDOWN,
            )

        if not re.match(USER_LINK_REGEX, link):
            return await update.message.reply_text(
                text=self.replies.COMMAND_LINK["DENIED"],
                parse_mode=ParseMode.MARKDOWN,
            )

        _request = requests.patch(f"{API_BASE_URL}/api/user/{update.message.from_user.id}", json={
            "link": link,
        })
        if _request.status_code == 409:
            return await update.message.reply_text(
                text=self.replies.COMMAND_LINK["ALREADY_EXIST"],
                parse_mode=ParseMode.MARKDOWN,
            )
        elif _request.status_code == 200:
            user = _request.json()

            await update.message.reply_text(
                text=self.replies.COMMAND_LINK["SUCCESS"].format(
                    bot_username=update.get_bot().username,
                    link=user["link"],
                ),
                parse_mode=ParseMode.MARKDOWN,
            )

    @auth
    async def welcome_command(self, update: Update, context: CallbackContext):
        """ Command to change the User's welcome message. """

        welcome = " ".join(context.args)

        if welcome == "":
            return await update.message.reply_text(
                text=self.replies.COMMAND_WELCOME["DEFAULT"],
                parse_mode=ParseMode.MARKDOWN,
            )

        if not match(USER_WELCOME_REGEX, welcome) and welcome != "clear":
            return await update.message.reply_text(
                text=self.replies.COMMAND_WELCOME["DENIED"],
                parse_mode=ParseMode.MARKDOWN,
            )

        _request = requests.patch(f"{API_BASE_URL}/api/user/{update.message.from_user.id}", json={
            "welcomeMessage": welcome,
        })

        if _request.status_code != 200:
            return await update.message.reply_text(
                text=self.replies.COMMAND_WELCOME["DENIED"],
                parse_mode=ParseMode.MARKDOWN,
            )

        user = _request.json()

        if welcome != "clear":
            await update.message.reply_text(
                text=self.replies.COMMAND_WELCOME["SUCCESS"],
                parse_mode=ParseMode.MARKDOWN,
            )

            await update.message.reply_text(
                text=user["welcomeMessage"],
                parse_mode=ParseMode.MARKDOWN,
            )
        else:
            await update.message.reply_text(
                text=self.replies.COMMAND_WELCOME["DELETED"],
                parse_mode=ParseMode.MARKDOWN,
            )

    @auth
    async def delete_command(self, update: Update, context: CallbackContext):
        """ Command to delete User's link. """

        user = requests.get(f"{API_BASE_URL}/api/user", params={
            "telegramId": update.message.from_user.id,
        })
        user = user.json()

        if user["link"] is None:
            return await update.message.reply_text(
                text=self.replies.COMMAND_DELETE["ALREADY_DELETED"],
                parse_mode=ParseMode.MARKDOWN,
            )

        requests.patch(f"{API_BASE_URL}/api/user/{update.message.from_user.id}", json={
            "link": "del",
        })

        await update.message.reply_text(
            text=self.replies.COMMAND_DELETE["SUCCESS"],
            parse_mode=ParseMode.MARKDOWN,
        )

    @auth
    @admin
    async def reveal_command(self, update: Update, context: CallbackContext):
        reply_message = update.message.reply_to_message

        if reply_message is None:
            return await update.message.reply_text(
                "Перешлите нужное анонимное сообщение и одновременно используйте эту команду."
            )

        author = requests.get(f"{API_BASE_URL}/api/user/author/{reply_message.message_id}")
        author = author.json()

        await self.reveal_author(
            update,
            context,
            recipient_id=update.message.from_user.id,
            author_id=author["telegramId"],
        )

    @auth
    async def handle_message(self, update: Update, context: CallbackContext):
        """ Handle user input. """

        # @TODO Remove when all possible errors are handled.
        try:
            if update.message.text == "/reveal":
                return

            if update.message.forward_origin and update.message.forward_origin.chat.id == int(self.env.TELEGRAM_STORAGE_CHANNEL_ID):
                author = requests.get(f"{API_BASE_URL}/api/user/author_from_storage/{update.message.forward_origin.message_id}")
                author = author.json()

                await self.reveal_author(
                    update,
                    context,
                    recipient_id=update.message.from_user.id,
                    author_id=author["telegramId"],
                )

                return

            receiver_link = self.redis.get(f"session:{update.message.from_user.id}:message")
            reply_message = update.message.reply_to_message

            # If the User has receiver link.
            if receiver_link is not None:
                delete = self.redis.delete(f"session:{update.message.from_user.id}:message")

                receiver = requests.get(f"{API_BASE_URL}/api/user", params={
                    "link": receiver_link,
                })

                if receiver.status_code == 404:
                    return await update.message.reply_text("Похоже получатель изменил или удалил ссылку.")
                elif receiver.status_code != 200:
                    return await self.handle_error(
                        update,
                        context,
                        f"FATAL: GET /api/user [link: {receiver_link}] ({receiver.status_code})"
                    )

            # If the User replied to the anonymous message.
            elif reply_message:
                # Get Author of the replied message.
                receiver = requests.get(f"{API_BASE_URL}/api/user/author/{reply_message.message_id}")

                if receiver.status_code != 200:
                    return await self.handle_error(
                        update,
                        context,
                        f"FATAL: GET /api/user/author/{reply_message.message_id} ({receiver.status_code})"
                    )

            else:
                return

            receiver = receiver.json()

            # Send notification about successfully sent anonymous message to the author.
            await update.message.reply_text(self.replies.EVENT_ANONYMOUS_MESSAGE_SENT)

            # Send message to the storage and reveal the message author.
            message_in_storage = await update.message.copy(self.env.TELEGRAM_STORAGE_CHANNEL_ID)
            await self.reveal_author(
                update,
                context,
                recipient_id=receiver["telegramId"],
                author_id=update.message.from_user.id,
                to_storage=True,
                storage_message_id=message_in_storage.message_id,
            )

            if receiver_link:
                # Send notification about new anonymous message to the recipient.
                await update.get_bot().send_message(
                    chat_id=receiver["telegramId"],
                    text=self.replies.EVENT_ANONYMOUS_MESSAGE_RECEIVED,
                    parse_mode=ParseMode.MARKDOWN,
                )

            message_in_recipient_chat = None
            if receiver_link:
                # Send anonymous message to the recipient.
                message_in_recipient_chat = await update.message.copy(receiver["telegramId"])
            elif reply_message:
                # Send anonymous message to the author by replied message.
                original_message = requests.get(f"{API_BASE_URL}/api/message", params={
                    "recipientId": update.message.from_user.id,
                    "recipientChatMessageId": reply_message.message_id,
                })
                original_message = original_message.json()

                # Catch "Message to be replied not found" error.
                try:
                    message_in_recipient_chat = await update.message.copy(
                        original_message["authorId"],
                        reply_to_message_id=original_message["authorChatMessageId"],
                    )
                except BadRequest:
                    await update.get_bot().send_message(
                        chat_id=original_message["authorId"],
                        text="Вам ответили на *удаленное сообщение*!",
                    )
                    message_in_recipient_chat = await update.message.copy(original_message["authorId"])

            # Reveal the message's author to the receiver with the "Special" role.
            if UserRoles.Special in UserRoles(receiver["roles"]):
                await self.reveal_author(
                    update,
                    context,
                    recipient_id=receiver["telegramId"],
                    author_id=update.message.from_user.id,
                )

            # Store all needed data in backend.
            requests.post(
                f"{API_BASE_URL}/api/message",
                json={
                    "authorChatMessageId": update.message.message_id,
                    "recipientChatMessageId": message_in_recipient_chat.message_id,
                    "storageMessageId": message_in_storage.message_id,
                    "authorId": update.message.from_user.id,
                    "recipientId": int(receiver["telegramId"]),
                    "body": update.message.text,
                }
            )
        except Exception:
            await update.message.copy(self.env.TELEGRAM_ERROR_NOTIFICATIONS_CHANNEL_ID)

            error = "Something strange happen.\n\nUID: `{uid}`\nMID: `{mid}`".format(
                uid=update.message.from_user.id,
                mid=update.message.message_id,
            )

            self.logger.error(error)

            await update.get_bot().send_message(
                chat_id=self.env.TELEGRAM_ERROR_NOTIFICATIONS_CHANNEL_ID,
                text=error,
                parse_mode=ParseMode.MARKDOWN,
            )

    async def reveal_author(
            self,
            update: Update,
            context: CallbackContext,
            recipient_id: int | str,
            author_id: int | str,
            to_storage: bool = False,
            storage_message_id: int = None,
    ):
        recipient = await update.get_bot().getChat(recipient_id)
        subject = await update.get_bot().getChat(author_id)

        chat_to_reveal = \
            await update.get_bot().getChat(self.env.TELEGRAM_STORAGE_CHANNEL_ID) \
            if to_storage \
            else recipient

        message_text = ""

        if to_storage:
            message_text += "*Author:*\n\n"

        subject_username = ('@' + escape_markdown(subject.username)) if subject.username else '-'
        subject_first_name = ('`' + escape_markdown(subject.first_name) + '`') if subject.first_name else '-'
        subject_last_name = ('`' + escape_markdown(subject.last_name) + '`') if subject.last_name else '-'
        subject_chat_link = "[Chat (web)](https://web.telegram.org/k/#{link})".format(
            link=('@' + escape_markdown(subject.username)) if subject.username else subject.id
        )

        message_text += "username: {username}\n".format(username=subject_username)
        message_text += "first name: {first_name}\n".format(first_name=subject_first_name)
        message_text += "last name: {last_name}\n\n".format(last_name=subject_last_name)
        if not to_storage:
            message_text += \
                "Если {subject} не запретил отправку сообщений от незнакомых пользователей ".format(
                    subject=
                    ("@" + escape_markdown(subject.username))
                    if subject.username
                    else "*" + escape_markdown(subject.first_name) + "*",
                ) + \
                "или у вас есть его контакт, то вы сможете открыть чат с ним в браузере:\n"

        message_text += subject_chat_link

        message_text += f"\n\nID: `{subject.id}`"

        if to_storage:
            message_text += "\n\n*Recipient:*\n\n"

            recipient_username = ('@' + escape_markdown(recipient.username)) if recipient.username else '-'
            recipient_first_name = ('`' + escape_markdown(recipient.first_name) + '`') if recipient.first_name else '-'
            recipient_last_name = ('`' + escape_markdown(recipient.last_name) + '`') if recipient.last_name else '-'
            recipient_chat_link = "[Chat (web)](https://web.telegram.org/k/#{link})".format(
                link=('@' + escape_markdown(recipient.username)) if recipient.username else recipient.id
            )

            message_text += "username: {username}\n".format(username=recipient_username)
            message_text += "first name: {first_name}\n".format(first_name=recipient_first_name)
            message_text += "last name: {last_name}\n\n".format(last_name=recipient_last_name)
            message_text += recipient_chat_link

            message_text += f"\n\nID: `{recipient.id}`"

            message_text += f"\n\nMessage ID: `{storage_message_id}`"

        avatars = []

        if subject.photo:
            subject_avatar = await subject.photo.get_small_file()

            media = InputMediaPhoto(
                media=bytes(await subject_avatar.download_as_bytearray()),
                filename="subject.png",
            )

            avatars.append(media)

        if to_storage and recipient.photo:
            recipient_avatar = await recipient.photo.get_small_file()

            media = InputMediaPhoto(
                media=bytes(await recipient_avatar.download_as_bytearray()),
                filename="recipient.png",
            )

            avatars.append(media)

        if len(avatars) != 0:
            await chat_to_reveal.send_media_group(
                media=avatars,
                caption=message_text,
                parse_mode=ParseMode.MARKDOWN,
            )
        else:
            if to_storage:
                message_text += "\n\navatar is hidden"

            await chat_to_reveal.send_message(
                message_text,
                parse_mode=ParseMode.MARKDOWN,
            )

    async def handle_error(self, update: Update, context: CallbackContext, message: str):
        """ Handle errors. """

        self.logger.error(message)

        await update.get_bot().send_message(
            chat_id=self.env.TELEGRAM_ERROR_NOTIFICATIONS_CHANNEL_ID,
            text=escape_markdown(message),
            parse_mode=ParseMode.MARKDOWN,
        )

        await update.message.reply_text(self.replies.ERROR)

    def run(self):
        self.bot.run_polling(allowed_updates=Update.ALL_TYPES)
