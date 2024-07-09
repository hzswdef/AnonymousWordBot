COMMAND = docker compose -f ./compose.yml --env-file ./.env

up:
	$(COMMAND) up -d

start:
	$(COMMAND) start

stop:
	$(COMMAND) stop

restart:
	$(COMMAND) stop
	$(COMMAND) start

# Other.
logs:
	$(COMMAND) logs -f

rm:
	$(COMMAND) stop
	$(COMMAND) rm --force
