import { LOGIN, LOGOUT } from "./actions";

const initialState = {
  user: null,
};

function login(state, action) {
  const user = action.payload.user;
  const newState = Object.assign({}, state, {});
  newState.user = user;
  return newState;
}

function logout(state, action) {
  const newState = Object.assign({}, state, {});
  newState.user = null;
  return newState;
}

function rootReducer(state = initialState, action) {
  if (action.type === LOGIN) {
    state = login(state, action);
  } else if (action.type === LOGOUT) {
    state = logout(state, action);
  }

  return state;
}

export default rootReducer;
